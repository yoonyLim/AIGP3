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

public class BaseAgent : MonoBehaviour, IAgent, IDamageable
{
    [SerializeField] protected AgentType agentType;

    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator animator;

    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] private GenericObserver<float> _currentHealth = new GenericObserver<float>(100f);
    // private float currentHealth;

    public Action OnDeath { get; set; }
    
    private static readonly Dictionary<AgentMoveType, float> moveSpeedMap = new() 
    {
        { AgentMoveType.Idle, 0f },
        { AgentMoveType.Patrol, 8f },
        { AgentMoveType.Strafe, 5f },
        { AgentMoveType.Chase, 15f },
        { AgentMoveType.Flee, 15f }
    };
    
    protected Coroutine cooldownCoroutine;
    
    protected IEnumerator ResetBool(string key, Blackboard blackboard, float duration)
    {
        yield return new WaitForSeconds(duration);
        blackboard.Set(key, true);
    }

    private void Start()
    {
        // currentHealth = maxHealth;
        _currentHealth.Invoke();
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
        float moveSpeed = moveSpeedMap.TryGetValue(moveType, out var speed) ? speed : 0f;
        Vector3 dir = (destination - transform.localPosition).normalized;
        Vector3 flatDir = new Vector3(dir.x, 0, dir.z).normalized;

        if (flatDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, 10 * Time.deltaTime));
            rb.MovePosition(rb.position + transform.forward * (moveSpeed * Time.deltaTime));
        }
    }

    public virtual bool HasArrived(Vector3 destination, float threshold)
    {
        return Vector3.Distance(transform.localPosition, destination) < threshold;
    }

    public virtual void Dodge(Vector3 movement)
    {
        rb.MovePosition(rb.position + movement);
    }
    
    // Strafe
    public void Strafe(Vector3 centerPos, float radius = 3f, float angularSpeed = 90f, int direction = 1)
    {
        Vector3 toSelf = new Vector3((transform.localPosition - centerPos).x, 0, (transform.localPosition - centerPos).z).normalized;

        float angleDelta = angularSpeed * Time.deltaTime * direction;
        Quaternion rotation = Quaternion.Euler(0, angleDelta, 0);
        Vector3 rotatedDir = rotation * toSelf;

        Vector3 nextPos = centerPos + rotatedDir * radius;
        Vector3 moveDir = new Vector3((nextPos - transform.localPosition).x, 0, (nextPos - transform.localPosition).z).normalized;

        float moveSpeed = GetMoveSpeed(AgentMoveType.Strafe);

        Vector3 lookDir = (centerPos - transform.localPosition).normalized;
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime));
        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.deltaTime));
    }

    public virtual void TakeDamage(float amount)
    {
        _currentHealth.Value -= amount;

        Debug.Log($"Take Damage, Current Health: {_currentHealth.Value:F2}");
        if (_currentHealth.Value <= 0f)
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
}
