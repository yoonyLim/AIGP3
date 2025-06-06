using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEditor.UI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class RLDefensiveAagent : Agent
{
    [Header("References")] 
    [SerializeField] public DefenseAgent selfAgent;
    [SerializeField] public AttackAgent targetAgent;
    
    [Header("Agent Properties")]
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] public float rotationSpeed = 180f;

    private Vector3 dodgeDirection;
    private Quaternion dodgeRotation;

    private Rigidbody rb;
    private float dodgeSpeed;
    private float dodgeDistance;
    private float prevDistanceToTarget;
    private Vector3 selfVelocity = Vector3.zero;
    private Vector3 targetVelocity = Vector3.zero;
    private Vector3 lastMoveVec;

    [SerializeField] private Renderer _groundRenderer;

    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;

    private Color _defaultGroundColor;
    private Coroutine _flashGroundCoroutine;

    public override void Initialize()
    {
        Debug.Log("Initialize");

        base.Initialize();

        rb = GetComponent<Rigidbody>();
        
        CurrentEpisode = 0;
        CumulativeReward = 0f;

        selfAgent.OnDodgeSucceeded += OnDodgeSucceededEvent;
        selfAgent.OnWallHit += OnWallHitEvent;
        selfAgent.OnCounterAttackSucceeded += OnCounterAttackSucceededEvent;
        selfAgent.OnBlockSucceeded += OnBlockSucceededEvent;
        selfAgent.OnBlockFailed += OnBlockFailedEvent;
        selfAgent.OnDamaged += OnDamagedEvent;
        selfAgent.OnDeath += OnSelfDeathEvent;
        
        targetAgent.OnDeath += OnTargetDeathEvent;

        if (_groundRenderer)
            _defaultGroundColor = _groundRenderer.material.color;
    }

    private void OnDodgeSucceededEvent()
    {
        AddReward(0.5f);
    }

    private void OnWallHitEvent()
    {
        AddReward(-0.5f);
    }
    
    private void OnCounterAttackSucceededEvent()
    {
        AddReward(0.6f);
    }

    private void OnBlockSucceededEvent()
    {
        AddReward(0.5f);
    }

    private void OnBlockFailedEvent()
    {
        AddReward(-0.4f);
    }

    private void OnDamagedEvent()
    {
        Debug.Log("OnDamagedEvent");
        AddReward(-0.5f);
    }

    private void OnSelfDeathEvent()
    {
        AddReward(-1f);
        EndEpisode();
    }
    
    private void OnTargetDeathEvent()
    {
        AddReward(1f);
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");
        
        dodgeSpeed = GameManager.Instance.GetDADodgeForce;
        dodgeDistance = GameManager.Instance.GetDADodgeDistance;
        prevDistanceToTarget = Vector3.Distance(selfAgent.GetLocalPos(), targetAgent.GetLocalPos());

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
        GameManager.Instance.IsEpisodeDone = false;
        
        selfAgent.ResetStatus();
        targetAgent.ResetStatus();
        
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0f, UnityEngine.Random.Range(0f, 9f));

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
        // Relative Positions
        Vector3 relativePos = targetAgent.GetLocalPos() - selfAgent.GetLocalPos();
        sensor.AddObservation(relativePos.x / 5f);
        sensor.AddObservation(relativePos.z / 5f);
        sensor.AddObservation(relativePos.magnitude / 10f); // normalize

        // Self Velocity
        MoveCommand? selfMoveCommand = selfAgent.GetMoveCommand();
        if (selfMoveCommand is { direction: not null }) selfVelocity = selfMoveCommand.Value.direction.Value * selfMoveCommand.Value.speed;
        sensor.AddObservation(selfVelocity.x / 10f);
        sensor.AddObservation(selfVelocity.z / 10f);
        
        // Target Velocity
        MoveCommand? targetMoveCommand = targetAgent.GetMoveCommand();
        if (targetMoveCommand is { direction: not null }) targetVelocity = targetMoveCommand.Value.direction.Value * targetMoveCommand.Value.speed;
        sensor.AddObservation(targetVelocity.x / 10f);
        sensor.AddObservation(targetVelocity.z / 10f);
        
        // Angle Between Agents
        Vector3 toTarget = (targetAgent.GetLocalPos() - transform.localPosition).normalized;
        float angleToTarget = Vector3.Dot(transform.forward, toTarget); // -1 to 1
        sensor.AddObservation(angleToTarget);
        
        // Attacker State
        sensor.AddObservation(targetAgent.IsAttacking ? 1f : 0f);
        
        // Cooldowns
        sensor.AddObservation(selfAgent.GetDodgeCooldown() / GameManager.Instance.GetDADodgeCooldown);
        sensor.AddObservation(selfAgent.GetAttackCooldown() / GameManager.Instance.GetDAAttackCooldown);
        sensor.AddObservation(selfAgent.GetBlockCooldown() / GameManager.Instance.GetDABlockCooldown);
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

        ProvideRewards();
    }

    public void CommandMovementAgent(int movementDecision)
    {
        switch (movementDecision)
        {
            case 0:
                // Do nothing - prevent blind action choices
                break;
            case 1:
                rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.deltaTime); // move forward
                lastMoveVec = transform.forward * moveSpeed;
                break;
            case 2:
                rb.MovePosition(rb.position + -transform.forward * moveSpeed * Time.deltaTime); // move backward
                lastMoveVec = -transform.forward * moveSpeed;
                break;
            case 3:
                rb.MovePosition(rb.position + transform.right * moveSpeed * Time.deltaTime); // move right
                lastMoveVec = transform.right * moveSpeed;
                break;
            case 4:
                rb.MovePosition(rb.position + -transform.right * moveSpeed * Time.deltaTime); // move left
                lastMoveVec = -transform.right * moveSpeed;
                break;
            case 5:
                rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, transform.up)); // rotate left
                break;
            case 6:
                rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, transform.up)); // rotate right
                break;
        }
    }

    private bool CanDodge()
    {
        return Mathf.Approximately(selfAgent.GetDodgeCooldown(), GameManager.Instance.GetDADodgeCooldown);
    }
    
    private bool CanAttack()
    {
        return Mathf.Approximately(selfAgent.GetAttackCooldown(), GameManager.Instance.GetDAAttackCooldown);
    }
    private bool CanBlock()
    {
        return Mathf.Approximately(selfAgent.GetBlockCooldown(), GameManager.Instance.GetDABlockCooldown);
    }
    

    public void CommandAttackAgent(int actionDecision)
    {
        switch (actionDecision)
        {
            case 0:
                // Do nothing - prevent blind action choices
                break;
            case 1:
                if (CanDodge())
                {
                    selfAgent.BeginDodge(targetAgent.GetLocalPos(), dodgeDistance, out dodgeDirection, out dodgeRotation);
                    selfAgent.TryDodge(dodgeDirection, dodgeRotation, dodgeSpeed);
                }
                else
                    AddReward(-0.01f);
                break;
            case 2:
                if (CanAttack())
                    selfAgent.CounterAttack();
                else
                    AddReward(-0.01f);
                break;
            case 3:
                if (CanBlock())
                    selfAgent.Block(targetAgent.GetLocalPos());
                else
                    AddReward(-0.01f);
                break;
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!CanDodge())
            actionMask.SetActionEnabled(1, 1, false); // disable dodge
        
        if (!CanAttack())
            actionMask.SetActionEnabled(1, 2, false);
        
        if (!CanBlock())
            actionMask.SetActionEnabled(1, 3, false);
    }

    private void ProvideRewards()
    {
        float currentDistanceToTarget = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        
        if (currentDistanceToTarget > prevDistanceToTarget)
            AddReward(0.002f);
        else
            AddReward(-0.001f);
        
        if (currentDistanceToTarget > 5f)
            AddReward(-0.001f);
        else if (currentDistanceToTarget > 3f)
            AddReward(0.001f);
        else if (currentDistanceToTarget < 2f)
            AddReward(-0.002f);
        
        Vector3 toTarget = targetAgent.GetLocalPos() - selfAgent.GetLocalPos();
        AddReward(0.001f * Vector3.Dot(transform.forward, toTarget)); // add reward if faccing the target
        
        if (selfAgent.IsNearWall() || selfVelocity.magnitude < 0.01f)
            AddReward(-0.005f);
        
        AddReward(-1f / 1000); // penalize as time takes too long to finish
        
        prevDistanceToTarget = currentDistanceToTarget;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-3f); // penalize for colliding with wall
            rb.MovePosition(rb.position - lastMoveVec * Time.deltaTime);
            // EndEpisode();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
            AddReward(-0.01f * Time.fixedDeltaTime); // penalize for the time of colliding with wall
    }
}
