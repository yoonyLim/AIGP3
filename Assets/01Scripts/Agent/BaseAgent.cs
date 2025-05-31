using UnityEngine;
using System.Collections.Generic;
using System;


public enum AgentMoveType
{
    Idle,
    Patrol,
    Chase,
    Flee
}

public enum AgentType
{
    None,
    Attack,
    Defense
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
        { AgentMoveType.Patrol, 3f },
        { AgentMoveType.Chase, 8f },
        { AgentMoveType.Flee, 8f }
    };

    private void Start()
    {
        currentHealth = maxHealth;
    }


    // Interface
    public AgentType GetAgentType()
    {
        return agentType;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public virtual void MoveTo(Vector3 destination, AgentMoveType moveType)
    {
        float moveSpeed = moveSpeedMap.TryGetValue(moveType, out var speed) ? speed : 0f;
        Vector3 dir = (destination - transform.position).normalized;
        Vector3 flatDir = new Vector3(dir.x, 0, dir.z).normalized;

        if (flatDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, 10 * Time.deltaTime));
            rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.deltaTime);
        }

        // TO DO: 애니메이션 실행
    }

    public virtual bool HasArrived(Vector3 destination, float threshold)
    {
        return Vector3.Distance(transform.position, destination) < threshold;
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

    public virtual void Die()
    {
        Debug.Log("죽음");
        OnDeath?.Invoke();
    }
}
