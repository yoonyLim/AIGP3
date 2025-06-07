using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
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
    [SerializeField] public float arenaHalfWidthHeight = 16f;
    
    [Header("Rewards")]
    [SerializeField] public float SuccessfulDodgeReward = 0.5f;
    [SerializeField] public float SuccessfulAttackReward = 1f;
    [SerializeField] public float SuccessfulBlockReward = 0.7f;
    [SerializeField] public float ExitWallReward = 0.5f;
    [SerializeField] public float FaceTargetReward = 0.005f;
    [SerializeField] public float FartherFromTargetReward = 0.002f;
    [SerializeField] public float IdealDistanceToTargetReward = 0.0005f;
    [SerializeField] public float WinReward = 3f;
    
    [Header("Penalties")]
    [SerializeField] public float WallHitPenalty = -0.4f;
    [SerializeField] public float ConstantWallHitPenalty = -0.01f;
    [SerializeField] public float FailedBlockPenalty = -0.4f;
    [SerializeField] public float FailedMovementPenalty = -0.01f;
    [SerializeField] public float DamagedPenalty = -0.5f;
    [SerializeField] public float TooCloseFarTargetPenalty = -0.001f;
    [SerializeField] public float TooCloseWallPenalty = -0.005f;
    [SerializeField] public float OutsideArenaPenalty = -1f;
    [SerializeField] public float LossPenalty = -1f;

    private Vector3 dodgeDirection;
    private Quaternion dodgeRotation;

    private Rigidbody rb;
    private float dodgeForce;
    private float dodgeDistance;
    private float dodgeDuration = 0.2f;
    private float dodgeElapsedTime = 0.2f;
    private bool isDodging = false;
    private bool hasRecentlyDodged = false;
    private float recentlyDodgedDuration = 2f;
    private float recentlyDodgedElapsedtime = 2f;
    private bool hasRecentlyBlocked = false;
    private float blockDuration = 2f;
    private float blockElapsedtime = 2f;
    private float prevDistanceToTarget;
    private int prevMoveDecision = 0;
    private float wallTouchingTime = 0f;
    private Vector3 selfVelocity = Vector3.zero;
    private Vector3 targetVelocity = Vector3.zero;

    [SerializeField] private Renderer _groundRenderer;

    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;

    private Color _defaultGroundColor;
    private Coroutine _flashGroundCoroutine;

    #region INITIALIZATION
    public override void Initialize()
    {
        // Debug.Log("Initialize");

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
    
    public override void OnEpisodeBegin()
    {
        // Debug.Log("Episode Begin");
        
        dodgeForce = GameManager.Instance.GetDADodgeForce;
        dodgeDistance = GameManager.Instance.GetDADodgeDistance;
        prevDistanceToTarget = Vector3.Distance(selfAgent.GetLocalPos(), targetAgent.GetLocalPos());
        dodgeElapsedTime = 1f;
        isDodging = false;
        hasRecentlyDodged = false;
        recentlyDodgedElapsedtime = 2f;
        hasRecentlyBlocked = false;
        blockElapsedtime = 1f;
        prevMoveDecision = 0;
        wallTouchingTime = 0f;

        if (_groundRenderer && CumulativeReward != 0f) // if previous episode was not a success, flash the ground color based on the cumulative reward
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
    #endregion

    #region EVENTS_REWARDS_PENALTIES
    private void OnDodgeSucceededEvent()
    {
        AddReward(SuccessfulDodgeReward);
    }

    private void OnWallHitEvent()
    {
        AddReward(WallHitPenalty);
    }
    
    private void OnCounterAttackSucceededEvent()
    {
        if (hasRecentlyBlocked)
        {
            AddReward(1f); // bonus reward for counter attack right after block
            hasRecentlyBlocked = false;
        }

        if (hasRecentlyDodged)
        {
            AddReward(1f);
            hasRecentlyDodged = false;
        }
        
        AddReward(SuccessfulAttackReward);
    }

    private void OnBlockSucceededEvent()
    {
        AddReward(SuccessfulBlockReward);
    }

    private void OnBlockFailedEvent()
    {
        AddReward(FailedBlockPenalty);
    }

    private void OnDamagedEvent()
    {
        AddReward(DamagedPenalty);
    }

    private void OnSelfDeathEvent()
    {
        AddReward(LossPenalty);
        CumulativeReward = GetCumulativeReward();
        EndEpisode();
    }
    
    private void OnTargetDeathEvent()
    {
        AddReward(WinReward);
        CumulativeReward = GetCumulativeReward();
        EndEpisode();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(WallHitPenalty); // penalize for colliding with wall
            // EndEpisode();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(ConstantWallHitPenalty * Time.fixedDeltaTime); // penalize for the time of colliding with wall
            wallTouchingTime += Time.fixedDeltaTime;
        }
    }
    
    // Reward for getting out of the wall
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(Mathf.Clamp(ExitWallReward - wallTouchingTime, 0.05f, ExitWallReward));
            wallTouchingTime = 0f;
        }
            
    }
    #endregion

    #region COMMANDS
    // the values to be passed into vector sensor works the bes in range [-1, 1] for machine learning
    // thus the need for normalization
    public override void CollectObservations(VectorSensor sensor)
    {
        // Relative Positions
        Vector3 relativePos = targetAgent.GetLocalPos() - selfAgent.GetLocalPos();
        sensor.AddObservation(relativePos.x / arenaHalfWidthHeight);
        sensor.AddObservation(relativePos.z / arenaHalfWidthHeight);
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
        else
            discreteActionsOut[0] = 0;
    }
    #endregion

    #region DISCRETE_ACTIONS_BRANCHES
    public override void OnActionReceived(ActionBuffers actions)
    {
        int movementDecision = actions.DiscreteActions[0];
        int actionDecision = actions.DiscreteActions[1];

        CommandAttackAgent(actionDecision); // call action first to see if dodge resets movement
        CommandMovementAgent(movementDecision);

        ProvideRewards();
        CumulativeReward = GetCumulativeReward();
    }
    
    public void CommandMovementAgent(int movementDecision)
    {
        switch (movementDecision)
        {
            case 0:
                // Do nothing - prevent blind action choices
                break;
            case 1:
                if (!selfAgent.TryMoveTo(rb.position + transform.forward * moveSpeed * Time.deltaTime, AgentMoveType.Flee)) // move backward
                    AddReward(FailedMovementPenalty); // Penalize for being stuck at wall
                break;
            case 2:
                if (!selfAgent.TryMoveTo(rb.position + -transform.forward * moveSpeed * Time.deltaTime, AgentMoveType.Flee)) // move backward
                    AddReward(FailedMovementPenalty); // Penalize for being stuck at wall
                break;
            case 3:
                if (!selfAgent.TryMoveTo(rb.position + transform.right * moveSpeed * Time.deltaTime, AgentMoveType.Flee)) // move right
                    AddReward(FailedMovementPenalty); // Penalize for being stuck at wall
                break;
            case 4:
                if (!selfAgent.TryMoveTo(rb.position + -transform.right * moveSpeed * Time.deltaTime, AgentMoveType.Flee)) // move left
                    AddReward(FailedMovementPenalty); // Penalize for being stuck at wall
                break;
            case 5:
                rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, transform.up)); // rotate left
                break;
            case 6:
                rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, transform.up)); // rotate right
                break;
        }
        
        if (movementDecision != prevMoveDecision)
            selfAgent.ResetMoveCommand();
        
        prevMoveDecision = movementDecision;

        // Penalize for going outside the arena
        if (Mathf.Abs(selfAgent.GetLocalPos().x) > arenaHalfWidthHeight || Mathf.Abs(selfAgent.GetLocalPos().z) > arenaHalfWidthHeight)
        {
            AddReward(OutsideArenaPenalty);
            EndEpisode();
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
        if (isDodging)
            dodgeElapsedTime += Time.fixedDeltaTime;
        
        if (hasRecentlyDodged)
            recentlyDodgedElapsedtime += Time.fixedDeltaTime;
        
        if (hasRecentlyBlocked)
            blockElapsedtime += Time.fixedDeltaTime;

        if (Mathf.Approximately(dodgeElapsedTime, dodgeDuration) && !isDodging)
        {
            isDodging = false;
            hasRecentlyDodged = true;
            recentlyDodgedElapsedtime = 0;
            selfAgent.ResetMoveCommand();
        }

        if (Mathf.Approximately(recentlyDodgedElapsedtime, recentlyDodgedDuration) && !hasRecentlyDodged)
            hasRecentlyDodged = false;

        if (Mathf.Approximately(blockElapsedtime, blockDuration) && !hasRecentlyBlocked)
            hasRecentlyBlocked = false;
        
        switch (actionDecision)
        {
            case 0:
                // Do nothing - prevent blind action choices
                break;
            case 1:
                if (CanDodge())
                {
                    selfAgent.BeginDodge(targetAgent.GetLocalPos(), dodgeDistance, out dodgeDirection, out dodgeRotation);
                    selfAgent.TryDodge(dodgeDirection, dodgeRotation, dodgeForce);
                    isDodging = true;
                    dodgeElapsedTime = 0f;
                }
                break;
            case 2:
                if (CanAttack())
                    selfAgent.CounterAttack();
                break;
            case 3:
                if (CanBlock())
                {
                    blockElapsedtime = 0f; 
                    hasRecentlyBlocked =  true;
                    selfAgent.Block(targetAgent.GetLocalPos());
                }
                break;
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!CanDodge())
            actionMask.SetActionEnabled(1, 1, false); // disable dodge
        
        if (!CanAttack())
            actionMask.SetActionEnabled(1, 2, false); // disable attack
        
        if (!CanBlock())
            actionMask.SetActionEnabled(1, 3, false); // disable block
    }
    #endregion
    
    #region ACTIONS_REWARDS_PENALTIES
    private float GetDistanceToClosestWall()
    {
        Vector3 pos = selfAgent.GetLocalPos();
        float distanceToLeft = Mathf.Abs(-arenaHalfWidthHeight - pos.x);
        float distanceToRight = Mathf.Abs(arenaHalfWidthHeight - pos.x);
        float distanceToBottom = Mathf.Abs(-arenaHalfWidthHeight - pos.z);
        float distanceToTop = Mathf.Abs(arenaHalfWidthHeight - pos.z);

        return Mathf.Min(distanceToLeft, distanceToRight, distanceToBottom, distanceToTop);
    }

    private void ProvideRewards()
    {
        float currentDistanceToTarget = Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition);
        
        // get farther away from the target
        if (currentDistanceToTarget > prevDistanceToTarget)
            AddReward(FartherFromTargetReward);
        else
            AddReward(TooCloseFarTargetPenalty);
        
        // keep an ideal distance with the target
        if (currentDistanceToTarget > 5f)
            AddReward(TooCloseFarTargetPenalty);
        else if (currentDistanceToTarget > 3f)
            AddReward(IdealDistanceToTargetReward);
        else if (currentDistanceToTarget < 2f)
            AddReward(TooCloseFarTargetPenalty);
        
        // please avoid the walls..
        float distanceToWall = GetDistanceToClosestWall();
        if (distanceToWall < 1f)
            AddReward(TooCloseWallPenalty);
        
        /*if (selfAgent.IsNearWall(2f) || selfVelocity.magnitude < 0.01f)
            AddReward(TooCloseWallPenalty);*/
        
        Vector3 toTarget = targetAgent.GetLocalPos() - selfAgent.GetLocalPos();
        AddReward(FaceTargetReward * Vector3.Dot(transform.forward, toTarget)); // add reward if faccing the target
        
        AddReward(-1f / 1000); // penalize as time takes too long to finish
        
        prevDistanceToTarget = currentDistanceToTarget;
    }
    #endregion
}
