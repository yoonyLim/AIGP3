using System;
using UnityEngine;

public class MoveToAction : ActionNode
{
    private IAgent _agent;
    private Func<Vector3> _destinationGetter;
    private AgentMoveType _moveType;
    private bool _hasArrived = false;

    public MoveToAction(IAgent agent, Func<Vector3> destinationGetter, AgentMoveType moveType) : base(null) 
    {
        _agent = agent;
        _destinationGetter = destinationGetter;
        _moveType = moveType;
    }

    public override INode.STATE Evaluate()
    {
        Debug.Log("MoveToAction started");
        
        if (_hasArrived)
            return INode.STATE.SUCCESS;
        
        Vector3 destination = _destinationGetter();
        _agent.MoveTo(destination, _moveType);

        if (_agent.HasArrived(destination))
        {
            Debug.Log("MoveToAction done");
            _hasArrived = true;
            return INode.STATE.SUCCESS;
        }
        
        return INode.STATE.RUN;
    }
}