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
    [SerializeField] public float SuccessfulAttackReward = 0.5f;
    [SerializeField] public float SuccessfulBlockReward = 3f;
    [SerializeField] public float ExitWallReward = 1f;
    [SerializeField] public float FaceTargetReward = 0.5f;
    [SerializeField] public float FartherFromTargetReward = 1f;
    [SerializeField] public float IdealDistanceToTargetReward = 1f;
    [SerializeField] public float WinReward = 5f;
    
    [Header("Penalties")]
    [SerializeField] public float WallHitPenalty = -1f;
    [SerializeField] public float ConstantWallHitPenalty = -0.01f;
    [SerializeField] public float FailedAttackPenalty = -1f;
    [SerializeField] public float FailedBlockPenalty = -0.5f;
    [SerializeField] public float FailedMovementPenalty = -0.1f;
    [SerializeField] public float DamagedPenalty = -0.5f;
    [SerializeField] public float TooCloseFarTargetPenalty = -1f;
    [SerializeField] public float TooCloseWallPenalty = -1f;
    [SerializeField] public float OutsideArenaPenalty = -3f;
    [SerializeField] public float LossPenalty = -5f;

    private Vector3 dodgeDirection; // for csv record
    private Quaternion dodgeRotation; // for csv record

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
    
    private bool isSelfDead = false; // for csv record
    private bool isTargetDead = false; // for csv record

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
        selfAgent.OnCounterAttackFailed += OnCounterAttackFailedEvent;
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
        
        if (CurrentEpisode >= 1)
        {
            selfAgent.WriteCSV("RLOffensive", (!isSelfDead && isTargetDead), (!isSelfDead && !isTargetDead), true, CumulativeReward);
            isSelfDead = false;
            isTargetDead = false;
        }

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
        transform.localPosition = new Vector3(0f, 0f, UnityEngine.Random.Range(1f, 12f));
        targetAgent.transform.localPosition = new Vector3(0f, 0f, UnityEngine.Random.Range(-12f, -1f));
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

    private void OnCounterAttackFailedEvent()
    {
        AddReward(FailedAttackPenalty);
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
        isSelfDead = true; // for csv record
        AddReward(LossPenalty);
        CumulativeReward = GetCumulativeReward();
        EndEpisode();
    }
    
    private void OnTargetDeathEvent()
    {
        isTargetDead = true; // for csv record
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
        // Target Health
        Vector3 relativePos = targetAgent.GetLocalPos() - selfAgent.GetLocalPos();
        sensor.AddObservation(relativePos.x / arenaHalfWidthHeight);
        sensor.AddObservation(relativePos.z / arenaHalfWidthHeight);
        sensor.AddObservation(relativePos.magnitude / 10f);
        
        // Distance to Target
        float distanceToTarget = Vector3.Distance(targetAgent.GetLocalPos(), selfAgent.GetLocalPos());
        sensor.AddObservation(distanceToTarget / 10f);
        
        // Target Health
        sensor.AddObservation(targetAgent.GetHealth() / 100f);

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
        
        // Self and Target's States
        sensor.AddObservation(selfAgent.IsAttacking ? 1f : 0f);
        sensor.AddObservation(targetAgent.IsBlocking ? 1f : 0f);
        
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

        CommandAction(actionDecision); // call action first to see if dodge resets movement
        CommandMovement(movementDecision);

        ProvideRewards();
        CumulativeReward = GetCumulativeReward();
    }
    
    public void CommandMovement(int movementDecision)
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
        
        // Heuristic only
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

    public void CommandAction(int actionDecision)
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
        float distanceDelta = currentDistanceToTarget - prevDistanceToTarget;
        if (distanceDelta > 0.05f)
            AddReward(FartherFromTargetReward * 0.1f);
        else if (distanceDelta < -0.05f)
            AddReward(TooCloseFarTargetPenalty * 0.1f);
        
        // keep an ideal distance with the target
        if (currentDistanceToTarget >= 3f && currentDistanceToTarget <= 5f)
            AddReward(IdealDistanceToTargetReward * 0.1f);
        else
            AddReward(TooCloseFarTargetPenalty * 0.1f);
        
        // face the target
        Vector3 toTarget = (targetAgent.GetLocalPos() - selfAgent.GetLocalPos()).normalized;
        float facingDot = Vector3.Dot(transform.forward, toTarget); // between -1 and 1
        if (facingDot > 0.9f)
            AddReward(FaceTargetReward * 0.1f); // only if mostly facing
        else
            AddReward(-FaceTargetReward * 0.05f); // small penalty for not facing
        
        // please avoid the walls..
        float distanceToWall = GetDistanceToClosestWall();
        if (distanceToWall < 1f)
            AddReward(TooCloseWallPenalty * 0.2f); // bigger penalty for being too close
        else if (distanceToWall < 2f)
            AddReward(TooCloseWallPenalty * 0.1f); // mild warning zone
        
        AddReward(-1f / 1000); // penalize as time takes too long to finish
        
        prevDistanceToTarget = currentDistanceToTarget;
    }
    #endregion
}
