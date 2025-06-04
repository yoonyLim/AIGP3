using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;


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

    [SerializeField] protected float maxHealth = 100f;
    private float currentHealth;

    public Action OnDeath { get; set; }
    
    private static readonly Dictionary<AgentMoveType, float> moveSpeedMap = new() 
    {
        { AgentMoveType.Idle, 0f },
        { AgentMoveType.Patrol, 2f },
        { AgentMoveType.Strafe, 1f },
        { AgentMoveType.Chase, 5f },
        { AgentMoveType.Flee, 5f }
    };

    private MoveCommand? moveCommand = null;

    protected Coroutine cooldownCoroutine;
    
    protected IEnumerator ResetBool(string key, Blackboard blackboard, float duration)
    {
        yield return new WaitForSeconds(duration);
        blackboard.Set(key, true);
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    protected float GetMoveSpeed(AgentMoveType moveType)
    {
        return moveSpeedMap.TryGetValue(moveType, out var speed) ? speed : 0f;
    }


    // Interface
    public BaseAgent GetAgent()
    {
        return this;
    }

    public AgentType GetAgentType()
    {
        return agentType;
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

    public virtual void MoveTo(Vector3 destination, AgentMoveType moveType)
    {
        float moveSpeed = GetMoveSpeed(moveType);
        Vector3 dir = (destination - transform.localPosition).normalized;
        Vector3 flatDir = new Vector3(dir.x, 0, dir.z).normalized;

        if (flatDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            moveCommand = new MoveCommand
            {
                direction = flatDir,
                speed = moveSpeed,
                rotation = targetRot
            };
        }
    }

    public virtual bool HasArrived(Vector3 destination, float threshold)
    {
        return Vector3.Distance(transform.localPosition, destination) < threshold;
    }

    public virtual void Dodge(Vector3 direction, float speed, DodgeType type)
    {
        if (type == DodgeType.Dodge)
        {
            animator.SetTrigger("Dodge");
        }
        else if (type == DodgeType.Dash)
        {
            animator.SetTrigger("Dash");
        }

        moveCommand = new MoveCommand
        {
            direction = direction,
            speed = speed,  
            rotation = null
        };
    }
    

    // Strafe
    public void Strafe(Vector3 centerPos, float radius = 3f, float angularSpeed = 90f, int direction = 1)
    {
        Vector3 toSelf = new Vector3((transform.localPosition - centerPos).x, 0, (transform.localPosition - centerPos).z).normalized;

        float angleDelta = angularSpeed * Time.fixedDeltaTime * direction;
        Quaternion rotation = Quaternion.Euler(0, angleDelta, 0);
        Vector3 rotatedDir = rotation * toSelf;

        Vector3 nextPos = centerPos + rotatedDir * radius;
        Vector3 moveDir = new Vector3((nextPos - transform.localPosition).x, 0, (nextPos - transform.localPosition).z).normalized;

        float moveSpeed = GetMoveSpeed(AgentMoveType.Strafe);
        Vector3 lookDir = (centerPos - transform.localPosition).normalized;
        Quaternion targetRot = Quaternion.LookRotation(lookDir);

        moveCommand = new MoveCommand
        {
            direction = moveDir,
            speed = moveSpeed,
            rotation = targetRot
        };
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;

        Debug.Log($"Take Damage, Current Health: {currentHealth:F2}");
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public virtual void ResetCooldown(string key, Blackboard blackboard, float duration)
    {
        StartCoroutine(ResetBool(key, blackboard, duration));
    }

    public virtual void Die()
    {
        Debug.Log("Á×À½");
        OnDeath?.Invoke();
    }


    public void ResetMoveCommand()
    {
        moveCommand = null;
    }

    public bool WillHitObstacle(Vector3 destination, float distance, LayerMask wallMask)
    {
        Vector3 origin = transform.position;
        Vector3 dir = (destination - origin).normalized;
        return Physics.Raycast(origin, dir, distance, wallMask);
    }

    private void FixedUpdate()
    {
        if (moveCommand.HasValue)
        {
            var cmd = moveCommand.Value;

            if (cmd.rotation.HasValue)
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, cmd.rotation.Value, 10 * Time.fixedDeltaTime));

            if (cmd.direction.HasValue)
                rb.MovePosition(rb.position + cmd.direction.Value * cmd.speed * Time.fixedDeltaTime);
        }
    }
}
