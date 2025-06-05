using System;
using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class RLAagent : Agent
{
    [Header("References")]
    public DefenseAgent targetAgent;            // 상대 에이전트
    public float moveSpeed = 2f;                // 이동 속도
    public float dashSpeed = 6f;                // 대시 속도
    public float dashDuration = 0.2f;           // 대시 지속 시간
    public Collider punchHitBox;
    public Collider kickHitBox;

    [Header("Cooldowns")]
    private float punchCooldown = 2.5f;
    private float kickCooldown = 2.5f;
    private float dashCooldown = 5f;

    private float punchTimer = 0f;
    private float kickTimer = 0f;
    private float dashTimer = 0f;

    private bool isDashing = false;
    private float dashTimerCurrent = 0f;

    Rigidbody _rigidbody;
    Animator _animator;


    [SerializeField] private Renderer _groundRenderer;

    private Renderer _renderer;

    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;

    private Color _defaultGroundColor;
    private Coroutine _flashGroundCoroutine;
    
    public override void Initialize()
    {
        Debug.Log("Initialize");
        
        base.Initialize();

        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        punchHitBox.enabled = false;
        kickHitBox.enabled = false;

        _renderer = GetComponent<Renderer>();
        CurrentEpisode = 0;
        CumulativeReward = 0f;
        
        if (_groundRenderer)
            _defaultGroundColor = _groundRenderer.material.color;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");

        if (_groundRenderer && CumulativeReward != 0f) // if previous episode was not a success, flash the ground color based on the cumulative reward
        {
            Color flashColor = (CumulativeReward > 0f) ? Color.green : Color.red;

            if (_flashGroundCoroutine != null)
                StopCoroutine(_flashGroundCoroutine);
            
            _flashGroundCoroutine = StartCoroutine(FlashGround(flashColor, 3f));
        }
        
        CurrentEpisode++;
        CumulativeReward = 0f;
        _renderer.material.color = Color.blue;

        SpawnObjects(); // reposition objects

        // 1) 내 위치 & 상대 위치 랜덤 초기화


        //transform.localPosition = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f));
        //targetAgent.localPosition(new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f)));

        //targetAgent.
        // 2) 쿨다운 초기화
        punchTimer = punchCooldown;
        kickTimer = kickCooldown;
        dashTimer = dashCooldown;
        isDashing = false;
        dashTimerCurrent = 0f;

        // 3) 물리 속도 초기화
        _rigidbody.linearVelocity = Vector3.zero;
    }

    private IEnumerator FlashGround(Color flashColor, float duration)
    {
        float elapsedTime = 0f;
        
        _groundRenderer.material.color = flashColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _groundRenderer.material.color = Color.Lerp(flashColor, _defaultGroundColor, elapsedTime / duration);
            yield return null;
        }
    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.15f, 0f);

        // random y-axis direction (angle in degrees)
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
        
        // random distance
        float randomDistance = Random.Range(1f, 2.5f);
        
        // goal's postion
        Vector3 targetAgentPosition = transform.localPosition + randomDirection * randomDistance;
        targetAgent.transform.localPosition = new Vector3(targetAgentPosition.x, 0.3f, targetAgentPosition.z);
     
    }
    
    // the values to be passed into vector sensor works the bes in range [-1, 1] for machine learning
    // thus the need for normalization
    public override void CollectObservations(VectorSensor sensor)
    {
        //// goal's position
        //float goalPosNormalizedX = _goal.localPosition.x / 5f;
        //float goalPosNormalizedZ = _goal.localPosition.z / 5f;

        //// turtle's position
        //float turtlePosNormalizedX = transform.localPosition.x / 5f;
        //float turtlePosNormalizedZ = transform.localPosition.z / 5f;

        //// turtle's rotation
        //float turtleRotationNormalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        //sensor.AddObservation(goalPosNormalizedX);
        //sensor.AddObservation(goalPosNormalizedZ);
        //sensor.AddObservation(turtlePosNormalizedX);
        //sensor.AddObservation(turtlePosNormalizedZ);
        //sensor.AddObservation(turtleRotationNormalized);

        // (A) 내-상대 상대 위치 (정규화): 상대 위치 - 내 위치, 2차원 거리(x,z)
        Vector3 dirToTarget = targetAgent.transform.localPosition - transform.localPosition;
        sensor.AddObservation(dirToTarget.normalized);         // 방향 벡터 (2개)
        sensor.AddObservation(dirToTarget.magnitude / 10f);    // 거리(0~1 범위로 정규화)

        // (B) 현재 내 이동 속도(벡터)
        sensor.AddObservation(_rigidbody.linearVelocity.x / moveSpeed);
        sensor.AddObservation(_rigidbody.linearVelocity.z / moveSpeed);

        // (C) 쿨다운 남은 시간 (0 ~ 1로 정규화)
        sensor.AddObservation(Mathf.Clamp01(punchTimer / punchCooldown));
        sensor.AddObservation(Mathf.Clamp01(kickTimer / kickCooldown));
        sensor.AddObservation(Mathf.Clamp01(dashTimer / dashCooldown));

        // (D) 대시 중인지 아닌지 (bool)
        sensor.AddObservation(isDashing ? 1f : 0f);

        // (E) 상대 체력 비율(원한다면)
        //sensor.AddObservation(targetAgent.CurrentHealth / targetAgent.MaxHealth);

        // 총 관찰 길이 = 2 (방향) +1(거리) +2(속도) +3(쿨다운) +1(대시 여부) +1(상대 체력) = 10 float
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.UpArrow))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.LeftArrow))
            discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.RightArrow))
            discreteActionsOut[0] = 3;
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        //MoveAgent(actions.DiscreteActions); // move turtle

        //AddReward(-2f / MaxStep); // penalize for taking actions

        //CumulativeReward = GetCumulativeReward(); // get cumulative reward
        // Discrete Action 예시: 
        // action[0]: 0=Idle, 1=앞으로(정면),2=뒤,3=왼쪽,4=오른쪽, 5=대시
        // action[1]: 0=행동 없음, 1=펀치, 2=킥

        int moveDecision = actions.DiscreteActions[0];
        int attackDecision = actions.DiscreteActions[1];

        Vector3 directionToTarget = targetAgent.transform.localPosition - transform.localPosition;
        directionToTarget.y = 0f; // 수직 축 회전 배제, 지면 평면( xz )만 사용

        // 1) 상대방을 바라보도록 회전 
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            // 즉시 회전: 
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            // Time.fixedDeltaTime * 10f 정도로 속도 조절 → 부드럽게 회전됨
        }

        Vector3 moveDir = Vector3.zero;
        switch (moveDecision)
        {
            case 1: moveDir = transform.forward; break;
            case 2: moveDir = -transform.forward; break;
            case 3: moveDir = -transform.right; break;
            case 4: moveDir = transform.right; break;
            case 5:
                if (dashTimer >= dashCooldown && !isDashing)
                {
                    isDashing = true;
                    dashTimerCurrent = 0f;
                    dashTimer = 0f;
                    _animator.SetTrigger("Dash");  // 대시 애니메이션
                }
                break;
            default:
                moveDir = Vector3.zero; break;
        }

        // 이동 처리(대시 중이 아니면 일반 이동)
        if (!isDashing)
        {
            _rigidbody.linearVelocity = moveDir * moveSpeed;
        }
        else
        {
            // 대시 지속 시간 동안 빠르게 이동
            dashTimerCurrent += Time.fixedDeltaTime;
            _rigidbody.linearVelocity = transform.forward * dashSpeed;
            if (dashTimerCurrent >= dashDuration)
            {
                isDashing = false;
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        // 공격 처리
        // 펀치
        if (attackDecision == 1 && punchTimer >= punchCooldown)
        {
            punchTimer = 0f;
            StartCoroutine(PunchRoutine());
        }
        // 킥
        else if (attackDecision == 2 && kickTimer >= kickCooldown)
        {
            kickTimer = 0f;
            StartCoroutine(KickRoutine());
        }

        // 쿨다운 시간 갱신
        punchTimer += Time.fixedDeltaTime;
        kickTimer += Time.fixedDeltaTime;
        dashTimer += Time.fixedDeltaTime;

        // Reward 부여
        ProvideRewards(moveDir, attackDecision);
    }

    //public void MoveAgent(ActionSegment<int> action)
    //{
    //    var chosenAction = action[0];

    //    switch (chosenAction)
    //    {
    //        case 1:
    //            transform.position += transform.forward * _moveSpeed * Time.deltaTime; // move forward
    //            break;
    //        case 2:
    //            transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f); // rotate left
    //            break;
    //        case 3:
    //            transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f); // rotate right
    //            break;
    //    }
    //}
    private void ProvideRewards(Vector3 moveDir, int attackDecision)
    {
        // 1) 상대와 가까워지는 걸 장려
        float prevDistance = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        // (Unity ML-Agents는 기본적으로 Update가 아니라 FixedUpdate 단위로 action이 들어오니까,
        //  여기에선 이동 후의 거리를 사용해서 간단히 차이를 측정해보거나, 같은 스텝의 마지막과 비교하는 식으로 구성해도 좋음.)
        float newDistance = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        if (newDistance < prevDistance)
            AddReward(0.002f);
        else
            AddReward(-0.001f);//수정 필요

        // 2) 펀치/킥 성공 시 보상 → OnHit 이벤트에서 주는 편이 깔끔
        //    여기서는 네트워크에 신호를 보내서 코루틴에서 호출하는 방식으로 구현

        // 3) 공격 회피(상대에게 공격당하지 않은 상태 유지) 보상
        //    예: 상대의 공격 판정 범위 안쪽으로 들어오다가도 대시로 빠져나오면 소량 보상

        // 4) 잘못된 행동(벽에 부딪히거나 가만히 있을 때) 페널티
        if (moveDir == Vector3.zero && attackDecision == 0 && !isDashing)
        {
            AddReward(-0.0005f);
        }

        // 5) 에피소드 종료 시 최종 보상
        //if (targetAgent.IsDead)
        //{
        //    AddReward(+1.0f);       // 상대를 쓰러뜨리면 큰 보상
        //    EndEpisode();
        //}
        //else if (this.IsDead)       // 맞아서 죽으면 벌점
        //{
        //    AddReward(-1.0f);
        //    EndEpisode();
        //}
        else if (StepCount > 1000)  // 최대 스텝 제한
        {
            EndEpisode();
        }
    }

    private IEnumerator PunchRoutine()
    {
        punchHitBox.enabled = true;
        _animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(0.5f);
        punchHitBox.enabled = false;
    }

    private IEnumerator KickRoutine()
    {
        kickHitBox.enabled = true;
        _animator.SetTrigger("Attack2");
        yield return new WaitForSeconds(1.0f);
        kickHitBox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.CompareTag("Goal"))
        //    GoalReached();

        // 상대가 공격해 올 때, 닿으면 데미지 → 나쁜 상황
        if (other.TryGetComponent<DefenseAgent>(out var def))
        {
            // (예시) 상대가 공격 애니메이션 중일 때 히트박스 판정 콜라이더를 켜놓으면, 여기서 감지
            //if (def.IsAttacking)
            //{
            //    AddReward(-0.8f);
            //}
        }
    }

    //private void GoalReached()
    //{
    //    AddReward(1f);
    //    CumulativeReward = GetCumulativeReward();
        
    //    EndEpisode();
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.05f); // penalize for colliding with wall
            
            if (_renderer)
                _renderer.material.color = Color.red;
        }

        // 상대가 방어 에이전트이면서 히트박스에 닿았을 때
        if (collision.collider == punchHitBox && collision.collider.TryGetComponent<DefenseAgent>(out var def1))
        {
            bool hit = def1.TakeDamage(5f);
            if (hit)
            {
                AddReward(+0.3f);  // 펀치 성공 보상
            }
        }
        if (collision.collider == kickHitBox && collision.collider.TryGetComponent<DefenseAgent>(out var def2))
        {
            bool hit = def2.TakeDamage(10f);
            if (hit)
            {
                AddReward(+0.5f);  // 킥 성공 보상
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
            AddReward(-0.01f * Time.fixedDeltaTime); // penalize for the time of colliding with wall
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && _renderer)
            _renderer.material.color = Color.blue;
    }
}
