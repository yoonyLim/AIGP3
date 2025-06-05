using System;
using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEditor.UI;
using Random = UnityEngine.Random;

public class RLDefensiveAagent : Agent
{
    [Header("References")] 
    [SerializeField] public DefenseAgent selfAgent;
    [SerializeField] public AttackAgent targetAgent;
    public float moveSpeed = 2f; 
    public float rotationSpeed = 180f;
    public float dodgeSpeed = 0.5f;
    public float dodgeDistance = 2f;

    [Header("Cooldowns")] private float punchCooldown = 2.5f;
    private float kickCooldown = 2.5f;
    private float dashCooldown = 5f;

    private float punchTimer = 0f;
    private float kickTimer = 0f;
    private float dashTimer = 0f;

    Rigidbody rb;
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

        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        CurrentEpisode = 0;
        CumulativeReward = 0f;

        if (_groundRenderer)
            _defaultGroundColor = _groundRenderer.material.color;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");

        if (_groundRenderer &&
            CumulativeReward !=
            0f) // if previous episode was not a success, flash the ground color based on the cumulative reward
        {
            Color flashColor = (CumulativeReward > 0f) ? Color.green : Color.red;

            if (_flashGroundCoroutine != null)
                StopCoroutine(_flashGroundCoroutine);

            _flashGroundCoroutine = StartCoroutine(FlashGround(flashColor, 3f));
        }

        CurrentEpisode++;
        CumulativeReward = 0f;

        SpawnObjects(); // reposition objects

        punchTimer = punchCooldown;
        kickTimer = kickCooldown;
        dashTimer = dashCooldown;
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
        transform.localPosition = new Vector3(0f, 0f, 0f);

        // random y-axis direction (angle in degrees)
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        // random distance
        float randomDistance = Random.Range(1f, 2.5f);

        // goal's postion
        Vector3 targetAgentPosition = transform.localPosition + randomDirection * randomDistance;
        targetAgent.transform.localPosition = new Vector3(targetAgentPosition.x, 0f, targetAgentPosition.z);
    }

    // the values to be passed into vector sensor works the bes in range [-1, 1] for machine learning
    // thus the need for normalization
    public override void CollectObservations(VectorSensor sensor)
    {
        // target agent's position
        float targetPosNormalizedX = targetAgent.GetLocalPos().x / 5f;
        float targetPosNormalizedZ = targetAgent.GetLocalPos().z / 5f;

        // self agent's postion
        float selfPosNormalizedX = transform.localPosition.x / 5f;
        float selfPosNormalizedZ = transform.localPosition.z / 5f;

        // self agent's rotation
        float selfRotationNormalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        sensor.AddObservation(targetPosNormalizedX);
        sensor.AddObservation(targetPosNormalizedZ);
        sensor.AddObservation(selfPosNormalizedX);
        sensor.AddObservation(selfPosNormalizedZ);
        sensor.AddObservation(selfRotationNormalized);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.UpArrow))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.DownArrow))
            discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.RightArrow))
            discreteActionsOut[0] = 3;
        else if (Input.GetKey(KeyCode.LeftArrow))
            discreteActionsOut[0] = 4;
        else if (Input.GetKey(KeyCode.Q))
            discreteActionsOut[0] = 5;
        else if (Input.GetKey(KeyCode.E))
            discreteActionsOut[0] = 6;
        else if (Input.GetKey(KeyCode.LeftControl))
            discreteActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.LeftShift))
            discreteActionsOut[1] = 2;
        else if (Input.GetKey(KeyCode.Space))
            discreteActionsOut[1] = 3;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int movementDecision = actions.DiscreteActions[0];
        int actionDecision = actions.DiscreteActions[1];

        CommandMovementAgent(movementDecision);
        CommandAttackAgent(actionDecision);

        // ProvideRewards(moveDir, actionDecision);
    }

    public void CommandMovementAgent(int movementDecision)
    {
        switch (movementDecision)
        {
            case 1:
                rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.deltaTime); // move forward
                break;
            case 2:
                rb.MovePosition(rb.position + -transform.forward * moveSpeed * Time.deltaTime); // move backward
                break;
            case 3:
                rb.MovePosition(rb.position + transform.right * moveSpeed * Time.deltaTime); // move right
                break;
            case 4:
                rb.MovePosition(rb.position + -transform.right * moveSpeed * Time.deltaTime); // move left
                break;
            case 5:
                rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, transform.up)); // rotate right
                break;
            case 6:
                rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, transform.up)); // rotate left
                break;
        }
    }

    public void CommandAttackAgent(int actionDecision)
    {
        switch (actionDecision)
        {
            case 1:
                selfAgent.TryDodge(targetAgent.GetLocalPos(), dodgeSpeed, dodgeDistance);
                break;
            case 2:
                selfAgent.Block(targetAgent.GetLocalPos());
                break;
            case 3:
                selfAgent.CounterAttack();
                break;
        }
    }

    private void ProvideRewards(Vector3 moveDir, int attackDecision)
    {
        float prevDistance = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        float newDistance = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        
        if (newDistance < prevDistance)
            AddReward(0.002f);
        else
            AddReward(-0.001f);
        
        if (moveDir == Vector3.zero && attackDecision == 0)
        {
            AddReward(-0.0005f);
        }
        
        else if (StepCount > 1000)
        {
            EndEpisode();
        }
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
