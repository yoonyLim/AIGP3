using System;
using UnityEngine;

public class FleeAction : ActionNode
{
    private IAgent self;
    private Func<Vector3> targetPositionGetter; 
    private AgentMoveType moveType;
    private float fleeDistance;

    public FleeAction(IAgent agent, Func<Vector3> targetPositionGetter, AgentMoveType moveType, float fleeDistance) : base(null)
    {
        self = agent;
        this.targetPositionGetter = targetPositionGetter;
        this.moveType = moveType;
        this.fleeDistance = fleeDistance;
    }

    public override INode.STATE Evaluate()
    {
        Vector3 targetPos = targetPositionGetter();
        Vector3 selfPos = self.GetLocalPos(); 
        Vector3 fleeDir = (selfPos - targetPos).normalized;

        Vector3 destination = selfPos + fleeDir * fleeDistance;

        bool canMove = self.TryMoveTo(destination, moveType);
        
        if (!canMove)
        {
            // Debug.Log("Flee, ���� �ε����� �����մϴ�");
            self.ResetMoveCommand();
            return INode.STATE.FAILED;
        }

        if (self.HasFled(destination, fleeDistance)) 
        {
            // Debug.Log("Flee");
            self.ResetMoveCommand();
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.RUN;
    }
}
