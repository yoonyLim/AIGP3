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
    public DefenseAgent targetAgent;            // ��� ������Ʈ
    public float moveSpeed = 2f;                // �̵� �ӵ�
    public float dashSpeed = 6f;                // ��� �ӵ�
    public float dashDuration = 0.2f;           // ��� ���� �ð�
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

        // 1) �� ��ġ & ��� ��ġ ���� �ʱ�ȭ


        //transform.localPosition = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f));
        //targetAgent.localPosition(new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f)));

        //targetAgent.
        // 2) ��ٿ� �ʱ�ȭ
        punchTimer = punchCooldown;
        kickTimer = kickCooldown;
        dashTimer = dashCooldown;
        isDashing = false;
        dashTimerCurrent = 0f;

        // 3) ���� �ӵ� �ʱ�ȭ
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

        // (A) ��-��� ��� ��ġ (����ȭ): ��� ��ġ - �� ��ġ, 2���� �Ÿ�(x,z)
        Vector3 dirToTarget = targetAgent.transform.localPosition - transform.localPosition;
        sensor.AddObservation(dirToTarget.normalized);         // ���� ���� (2��)
        sensor.AddObservation(dirToTarget.magnitude / 10f);    // �Ÿ�(0~1 ������ ����ȭ)

        // (B) ���� �� �̵� �ӵ�(����)
        sensor.AddObservation(_rigidbody.linearVelocity.x / moveSpeed);
        sensor.AddObservation(_rigidbody.linearVelocity.z / moveSpeed);

        // (C) ��ٿ� ���� �ð� (0 ~ 1�� ����ȭ)
        sensor.AddObservation(Mathf.Clamp01(punchTimer / punchCooldown));
        sensor.AddObservation(Mathf.Clamp01(kickTimer / kickCooldown));
        sensor.AddObservation(Mathf.Clamp01(dashTimer / dashCooldown));

        // (D) ��� ������ �ƴ��� (bool)
        sensor.AddObservation(isDashing ? 1f : 0f);

        // (E) ��� ü�� ����(���Ѵٸ�)
        //sensor.AddObservation(targetAgent.CurrentHealth / targetAgent.MaxHealth);

        // �� ���� ���� = 2 (����) +1(�Ÿ�) +2(�ӵ�) +3(��ٿ�) +1(��� ����) +1(��� ü��) = 10 float
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
        // Discrete Action ����: 
        // action[0]: 0=Idle, 1=������(����),2=��,3=����,4=������, 5=���
        // action[1]: 0=�ൿ ����, 1=��ġ, 2=ű

        int moveDecision = actions.DiscreteActions[0];
        int attackDecision = actions.DiscreteActions[1];

        Vector3 directionToTarget = targetAgent.transform.localPosition - transform.localPosition;
        directionToTarget.y = 0f; // ���� �� ȸ�� ����, ���� ���( xz )�� ���

        // 1) ������ �ٶ󺸵��� ȸ�� 
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            // ��� ȸ��: 
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            // Time.fixedDeltaTime * 10f ������ �ӵ� ���� �� �ε巴�� ȸ����
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
                    _animator.SetTrigger("Dash");  // ��� �ִϸ��̼�
                }
                break;
            default:
                moveDir = Vector3.zero; break;
        }

        // �̵� ó��(��� ���� �ƴϸ� �Ϲ� �̵�)
        if (!isDashing)
        {
            _rigidbody.linearVelocity = moveDir * moveSpeed;
        }
        else
        {
            // ��� ���� �ð� ���� ������ �̵�
            dashTimerCurrent += Time.fixedDeltaTime;
            _rigidbody.linearVelocity = transform.forward * dashSpeed;
            if (dashTimerCurrent >= dashDuration)
            {
                isDashing = false;
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        // ���� ó��
        // ��ġ
        if (attackDecision == 1 && punchTimer >= punchCooldown)
        {
            punchTimer = 0f;
            StartCoroutine(PunchRoutine());
        }
        // ű
        else if (attackDecision == 2 && kickTimer >= kickCooldown)
        {
            kickTimer = 0f;
            StartCoroutine(KickRoutine());
        }

        // ��ٿ� �ð� ����
        punchTimer += Time.fixedDeltaTime;
        kickTimer += Time.fixedDeltaTime;
        dashTimer += Time.fixedDeltaTime;

        // Reward �ο�
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
        // 1) ���� ��������� �� ���
        float prevDistance = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        // (Unity ML-Agents�� �⺻������ Update�� �ƴ϶� FixedUpdate ������ action�� �����ϱ�,
        //  ���⿡�� �̵� ���� �Ÿ��� ����ؼ� ������ ���̸� �����غ��ų�, ���� ������ �������� ���ϴ� ������ �����ص� ����.)
        float newDistance = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        if (newDistance < prevDistance)
            AddReward(0.002f);
        else
            AddReward(-0.001f);//���� �ʿ�

        // 2) ��ġ/ű ���� �� ���� �� OnHit �̺�Ʈ���� �ִ� ���� ���
        //    ���⼭�� ��Ʈ��ũ�� ��ȣ�� ������ �ڷ�ƾ���� ȣ���ϴ� ������� ����

        // 3) ���� ȸ��(��뿡�� ���ݴ����� ���� ���� ����) ����
        //    ��: ����� ���� ���� ���� �������� �����ٰ��� ��÷� ���������� �ҷ� ����

        // 4) �߸��� �ൿ(���� �ε����ų� ������ ���� ��) ���Ƽ
        if (moveDir == Vector3.zero && attackDecision == 0 && !isDashing)
        {
            AddReward(-0.0005f);
        }

        // 5) ���Ǽҵ� ���� �� ���� ����
        //if (targetAgent.IsDead)
        //{
        //    AddReward(+1.0f);       // ��븦 �����߸��� ū ����
        //    EndEpisode();
        //}
        //else if (this.IsDead)       // �¾Ƽ� ������ ����
        //{
        //    AddReward(-1.0f);
        //    EndEpisode();
        //}
        else if (StepCount > 1000)  // �ִ� ���� ����
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

        // ��밡 ������ �� ��, ������ ������ �� ���� ��Ȳ
        if (other.TryGetComponent<DefenseAgent>(out var def))
        {
            // (����) ��밡 ���� �ִϸ��̼� ���� �� ��Ʈ�ڽ� ���� �ݶ��̴��� �ѳ�����, ���⼭ ����
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

        // ��밡 ��� ������Ʈ�̸鼭 ��Ʈ�ڽ��� ����� ��
        if (collision.collider == punchHitBox && collision.collider.TryGetComponent<DefenseAgent>(out var def1))
        {
            bool hit = def1.TakeDamage(5f);
            if (hit)
            {
                AddReward(+0.3f);  // ��ġ ���� ����
            }
        }
        if (collision.collider == kickHitBox && collision.collider.TryGetComponent<DefenseAgent>(out var def2))
        {
            bool hit = def2.TakeDamage(10f);
            if (hit)
            {
                AddReward(+0.5f);  // ű ���� ����
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
