using System;
using UnityEngine;


public class RandomPatrolNode : ActionNode
{
    private IAgent agent;
    private float radius; // reacable radius
    private AgentMoveType moveType;
    
    private Vector3 currentTargetPoint;
    private bool hasTargetPoint;
    private int groundMask;


    public RandomPatrolNode(IAgent agent, AgentMoveType moveType, float radius = 5f) : base(null)
    {
        this.agent = agent;
        this.radius = radius;
        this.moveType = moveType;

        currentTargetPoint = Vector3.zero;
        hasTargetPoint = false;
        groundMask = LayerMask.GetMask("Ground");
    }


    public override INode.STATE Evaluate()
    {
        if (!hasTargetPoint || agent.HasArrived(currentTargetPoint))
        {
            if (TryGetRandomPoint(out Vector3 newTargetPoint))
            {
                currentTargetPoint = newTargetPoint;
                hasTargetPoint = true;
            }
            else
            {
                return INode.STATE.FAILED;
            }
        }

        agent.MoveTo(currentTargetPoint, AgentMoveType.Patrol);
        return INode.STATE.RUN;
    }


    private bool TryGetRandomPoint(out Vector3 point)
    {
        Vector2 randomOffset2D = UnityEngine.Random.insideUnitCircle * radius;

        Vector3 origin = agent.GetPosition();
        Vector3 randomPos = origin + new Vector3(randomOffset2D.x, 5f, randomOffset2D.y);

        if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 10f, groundMask))
        {
            point = hit.point;
            return true;
        }

        point = Vector3.zero;
        return false;
    }
}
