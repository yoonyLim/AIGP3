using UnityEngine;
using System.Collections.Generic;

public enum AgentMoveType
{
    Idle,
    Patrol,
    Chase,
    Flee
}

public class BaseAgent : MonoBehaviour, IAgent
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator animator;


    private static readonly Dictionary<AgentMoveType, float> moveSpeedMap = new() 
    {
        { AgentMoveType.Idle, 0f },
        { AgentMoveType.Patrol, 0.1f },
        { AgentMoveType.Chase, 0.5f },
        { AgentMoveType.Flee, 0.5f }
    };


    public virtual void MoveTo(Vector3 destination, AgentMoveType moveType)
    {
        float moveSpeed = moveSpeedMap.TryGetValue(moveType, out var speed) ? speed : 0f;
        Vector3 dir = (destination - transform.position).normalized;
        Vector3 flatDir = new Vector3(dir.x, 0, dir.x).normalized;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10 * Time.deltaTime);
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }

        // TO DO: 애니메이션 실행
    }

    public virtual bool HasArrived(Vector3 destination, float threshold)
    {
        return Vector3.Distance(transform.position, destination) < threshold;
    }
}
