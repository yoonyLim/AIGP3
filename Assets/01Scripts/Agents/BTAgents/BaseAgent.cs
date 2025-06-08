using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEditor;


public enum AgentMoveType
{
    Idle,
    Patrol,
    Chase,
    Flee,
    Strafe
}

public enum AgentType
{
    None,
    Attack,
    Defense
}

public enum DodgeType 
{   Dodge, 
    Dash 
}

public struct MoveCommand
{
    public float speed;
    public Vector3? direction;
    public Quaternion? rotation;
}


public class BaseAgent : MonoBehaviour, IAgent, IDamageable
{
    [SerializeField] protected AgentType agentType;

    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator animator;
    private LayerMask _wallMask;
    
    [Header("UI Observer")]
    [SerializeField] private GenericObserver<float> _currentHealth = new GenericObserver<float>(100f);
    [SerializeField] protected GenericObserver<float> _dodgeCooldown = new GenericObserver<float>(0f); // to be initialized separately in each attack and defense agents
    [SerializeField] protected GenericObserver<float> _attackCooldown = new GenericObserver<float>(0f); // to be initialized separately in each attack and defense agents
    [SerializeField] protected GenericObserver<float> _blockCooldown = new GenericObserver<float>(0f); // to be initialized only in defense agents
    [SerializeField] protected float blockDuration = 1f;

    public float GetDodgeCooldown() => _dodgeCooldown.Value;
    public float GetAttackCooldown() => _attackCooldown.Value;
    public float GetBlockCooldown() => _blockCooldown.Value;
    
    public bool IsAttacking { get; protected set; }
    public bool IsBlocking { get; protected set; }
    public bool IsDead { get; protected set; }
    
    public Action OnDodgeSucceeded { get; set; }
    public Action OnWallHit { get; set; }
    public Action OnDamaged { get; set; }
    public Action OnDeath { get; set; }
    public event Action OnBlockFailed;
    
    // for CSV data
    [Header("CSV Data")]
    public int numDodges = 0;
    public int numSuccessfulDodges = 0;
    public int numFailedDodges = 0;
    
    public int numAttacks = 0;
    public int numSuccessfulAttacks = 0;
    public int numFailedAttacks = 0;
    
    public int numBlocks = 0;
    public int numSuccessfulBlocks = 0;
    public int numFailedBlocks = 0;

    public bool HasWrittenCSV { get; protected set; }
    
    private static readonly Dictionary<AgentMoveType, float> moveSpeedMap = new() 
    {
        { AgentMoveType.Idle, 0f },
        { AgentMoveType.Patrol, 2f },
        { AgentMoveType.Strafe, 1f },
        { AgentMoveType.Chase, 5f },
        { AgentMoveType.Flee, 5f }
    };

    private static readonly int DashTrigger = Animator.StringToHash("Dash");
    private static readonly int DodgeTrigger = Animator.StringToHash("Dodge");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int GroundSpeed = Animator.StringToHash("Speed");
    private static readonly int BlockStart = Animator.StringToHash("BlockStart");
    private static readonly int BlockEnd = Animator.StringToHash("BlockEnd");

    private MoveCommand? moveCommand = null;

    protected virtual void Start()
    {
        _currentHealth.Invoke();
        _dodgeCooldown.Invoke();
        _attackCooldown.Invoke();
        _blockCooldown.Invoke();
        
        _wallMask = LayerMask.GetMask("Wall");
        
        IsAttacking = false;
        IsBlocking = false;
        IsDead = false;
    }

    public virtual void ResetStatus()
    {
        HasWrittenCSV = false;
        ResetMoveCommand();
        animator.SetBool(DieTrigger, false);
        _currentHealth.Value = 100f;
    }

    #region GETTER
    protected float GetMoveSpeed(AgentMoveType moveType)
    {
        return moveSpeedMap.TryGetValue(moveType, out var speed) ? speed : 0f;
    }

    public BaseAgent GetAgent()
    {
        return this;
    }

    public AgentType GetAgentType()
    {
        return agentType;
    }

    public Vector3 GetForward()
    {
        return transform.forward;
    }
    
    public Quaternion GetLocalRot()
    {
        return transform.localRotation;
    }

    public Vector3 GetLocalPos()
    {
        return transform.localPosition;
    }

    public Vector3 GetWorldPos()
    {
        return transform.position;
    }

    public virtual bool HasArrived(Vector3 destination, float threshold)
    {
        return Vector3.Distance(transform.localPosition, destination) < threshold;
    }

    public virtual bool HasFled(Vector3 destination, float threshold)
    {
        return Vector3.Distance(transform.localPosition, destination) > threshold;
    }

    #endregion

    #region MOVEMENT   
    /*public virtual bool TryMoveTo(Vector3 destination, AgentMoveType moveType, out Vector3 moveDir, out Quaternion rotation)
    {
        float moveSpeed = GetMoveSpeed(moveType);
        Vector3 dir = (destination - transform.localPosition).normalized;
        Vector3 flatDir = new Vector3(dir.x, 0, dir.z).normalized;

        Vector3 nextPos = transform.localPosition + flatDir * moveSpeed * Time.fixedDeltaTime;
        Vector3 checkCenter = nextPos + Vector3.up * 0.75f;

        if (IsPathBlocked(checkCenter))
        {
            moveDir = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        moveDir = flatDir;
        rotation = Quaternion.LookRotation(flatDir);
        return true;
    }*/

    public virtual bool TryMoveTo(Vector3 destination, AgentMoveType moveType)
    {
        float moveSpeed = GetMoveSpeed(moveType);
        Vector3 dir = (destination - transform.localPosition).normalized;
        Vector3 flatDir = new Vector3(dir.x, 0, dir.z).normalized;
        
        Vector3 nextPos = transform.localPosition + flatDir * (moveSpeed * Time.fixedDeltaTime * 15f);
        Vector3 checkCenter = nextPos + Vector3.up * 0.75f;

        if (IsPathBlocked(checkCenter))
        {
            Debug.Log("Wall Blocked");
            ResetMoveCommand();
            return false;
        }

        MoveTo(flatDir, moveSpeed);
        return true;
    }

    private void MoveTo(Vector3 moveDir, float speed)
    {
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            moveCommand = new MoveCommand
            {
                direction = moveDir,
                speed = speed,
                rotation = targetRot
            };
        }
    }

    public virtual void RotateTo(Quaternion targetRotation)
    {
        if (targetRotation != Quaternion.identity)
        {
            moveCommand = new MoveCommand
            {
                direction = null,
                speed = 0f,
                rotation = targetRotation
            };
        }
    }


    // Dash
    public void BeginDash(Vector3 targetPos, out Vector3 dashDir)
    {
        Vector3 dir = targetPos - transform.localPosition;
        dir.y = 0f;
        dashDir = dir.normalized;

        animator.SetTrigger(DashTrigger);
        _dodgeCooldown.Value = 0f;
    }

    public bool TryDash(Vector3 dashDir, float speed)
    {
        numDodges++; // for csv record
        Vector3 nextPos = transform.localPosition + dashDir * (speed * Time.fixedDeltaTime);
        Vector3 checkCenter = nextPos + Vector3.up * 0.75f;

        if (IsPathBlocked(checkCenter))
        {
            numFailedDodges++; // for csv record
            return false;
        }

        Dash(dashDir, speed);
        return true;
    }

    private void Dash(Vector3 direction, float speed)
    {
        numSuccessfulDodges++; // for csv record
        
        moveCommand = new MoveCommand
        {
            direction = direction,
            speed = speed,
            rotation = null
        };
    }

    // Dodge
    public void BeginDodge(Vector3 targetPos, float checkDistance, out Vector3 dodgeDir, out Quaternion dodgeLookRot)
    {
        Vector3 toTarget = targetPos - transform.localPosition;
        toTarget.y = 0f;
        Vector3 backDir = -toTarget.normalized;

        float angle = UnityEngine.Random.Range(-90f, 90f);
        dodgeDir = Quaternion.Euler(0, angle, 0) * backDir;
        dodgeLookRot = Quaternion.LookRotation(toTarget.normalized);

        animator.SetTrigger(DodgeTrigger);
        _dodgeCooldown.Value = 0f;
    }

    public bool TryDodge(Vector3 dodgeDir, Quaternion lookRot, float speed)
    {
        numDodges++; // for csv record
        Vector3 nextPos = transform.localPosition + dodgeDir * (speed * Time.fixedDeltaTime);
        Vector3 checkCenter = nextPos + Vector3.up * 0.75f;

        if (IsPathBlocked(checkCenter))
        {
            numFailedDodges++; // for csv record
            OnWallHit?.Invoke();
            return false;
        }

        Dodge(dodgeDir, speed, lookRot);
        
        return true;
    }

    private void Dodge(Vector3 direction, float speed, Quaternion faceRotation)
    {
        numSuccessfulDodges++; // for csv record
        OnDodgeSucceeded?.Invoke();
        
        moveCommand = new MoveCommand
        {
            direction = direction,
            speed = speed,
            rotation = faceRotation
        };
    }

    // Strafe
    public virtual bool TryStrafe(Vector3 centerPos, float radius, float angularSpeed , int initialDirection, out int usedDirection)
    {
        for (int i = 0; i < 2; i++)
        {
            int direction = (i == 0) ? initialDirection : -initialDirection;

            Vector3 toSelf = new Vector3((transform.localPosition - centerPos).x, 0, (transform.localPosition - centerPos).z).normalized;
            float angleDelta = angularSpeed * Time.fixedDeltaTime * direction;
            Quaternion rotation = Quaternion.Euler(0, angleDelta, 0);
            Vector3 rotatedDir = rotation * toSelf;

            Vector3 nextPos = centerPos + rotatedDir * radius;
            Vector3 checkCenter = nextPos + Vector3.up * 0.75f;

            if (IsPathBlocked(checkCenter))
                continue;

            Vector3 moveDir = (nextPos - transform.localPosition).normalized;
            Vector3 lookDir = (centerPos - transform.localPosition).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(lookDir);

            Strafe(moveDir, lookRotation);
            usedDirection = direction;
            
            return true;
        }

        usedDirection = 0;
        return false;
    }

    private void Strafe(Vector3 moveDir, Quaternion lookRotation)
    {
        moveCommand = new MoveCommand
        {
            direction = moveDir,
            speed = GetMoveSpeed(AgentMoveType.Strafe),
            rotation = lookRotation
        };
    }

    public bool IsPathBlocked(Vector3 center)
    {
        return Physics.CheckSphere(center, 0.1f, _wallMask);
    }

    public bool IsNearWall(float checkDistance = 3f)
    {
        RaycastHit hit;
        
        Vector3[] directions = {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right
        };

        foreach (var dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out hit, checkDistance))
            {
                if (hit.collider.CompareTag("Wall")) // make sure walls are tagged
                    return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Vector3 center = transform.localPosition + Vector3.up * 1.0f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, 0.3f);
    }

    public void ResetMoveCommand()
    {
        moveCommand = null;
    }
    
    private void FixedUpdate()
    {
        if (moveCommand.HasValue)
        {
            var cmd = moveCommand.Value;

            if (cmd.rotation.HasValue)
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, cmd.rotation.Value, 5 * Time.fixedDeltaTime));

            if (cmd.direction.HasValue)
                rb.MovePosition(rb.position + cmd.direction.Value * (cmd.speed * Time.fixedDeltaTime));
        }
    }
    #endregion
    
    public virtual bool TakeDamage(float amount)
    {
        OnDamaged?.Invoke();
        _currentHealth.Value -= amount;
        
        if (_currentHealth.Value <= 0f)
            Die();

        return true;
    }
    
    protected IEnumerator ResetBool(string key, Blackboard blackboard, float duration)
    {
        yield return new WaitForSeconds(duration);
        blackboard.Set(key, true);
    }

    public virtual void ResetCooldown(string key, Blackboard blackboard, float duration)
    {
        StartCoroutine(ResetBool(key, blackboard, duration));
    }
    
    private IEnumerator BlockCoroutine()
    {
        yield return new WaitForSeconds(blockDuration);
        
        if (IsBlocking)
        {
            IsBlocking = false;
            animator.SetTrigger(BlockEnd);
            numFailedBlocks++; // for csv record
            OnBlockFailed?.Invoke();
        }
    }
    
    public void Block(Vector3 targetPos)
    {
        numBlocks++; // for csv record
        IsBlocking = true;
        Vector3 dir = targetPos - transform.localPosition;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            rb.MoveRotation(rot);
        }

        _blockCooldown.Value = 0f;
        
        animator.SetTrigger(BlockStart);
        StartCoroutine(BlockCoroutine());
    }

    public virtual void Die()
    {
        IsDead = true;
        animator.SetBool(DieTrigger, true);
        GameManager.Instance.IsEpisodeDone = true;
        ResetMoveCommand();
        OnDeath?.Invoke();
    }

    public MoveCommand? GetMoveCommand()
    {
        return moveCommand;
    }

    protected virtual void Update()
    {
        animator.SetFloat(GroundSpeed, rb.linearVelocity.magnitude);
    }
    
    public void WriteCSV(string ID, bool didWin)
    {
        HasWrittenCSV = true;
        
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(ID + "Test.csv", true))
            {
                string battleRes = didWin ? "WIN" : "LOSS";
                
                file.WriteLine(ID + " Simulation Result");
                file.WriteLine("Total#ofDodges,#ofSuccessfulDodges,#ofFailedDodges,Total#ofAttacks,#ofSuccessfulAttacks,#ofFailedAttacks,Total#ofBlocks,#ofSuccessfulBlocks,BattleResult");
                file.WriteLine(numDodges + "," + numSuccessfulDodges + "," + numFailedDodges + "," + numAttacks + "," + numSuccessfulAttacks + "," + numFailedAttacks + "," + numBlocks + "," + numSuccessfulBlocks + "," + numFailedBlocks + "," + battleRes);
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Program suspended: " + e.Message);
        }
    }
}
