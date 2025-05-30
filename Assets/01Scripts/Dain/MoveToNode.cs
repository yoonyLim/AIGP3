using System;
using UnityEngine;

public class MoveToNode : ActionNode
{
    private IAgent agent;
    private Vector3 destination;
    private AgentMoveType moveType;


    public MoveToNode(IAgent agent, Vector3 destination, AgentMoveType moveType) : base(null) 
    {
        this.agent = agent;
        this.destination = destination;
        this.moveType = moveType;
    }

    public override INode.STATE Evaluate()
    {
        agent.MoveTo(destination, moveType);

        if (agent.HasArrived(destination))
            return INode.STATE.SUCCESS;
        else
            return INode.STATE.RUN;
    }
}