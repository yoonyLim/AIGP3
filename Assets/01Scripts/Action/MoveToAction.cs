using System;
using UnityEngine;

public class MoveToAction : ActionNode
{
    private IAgent _selfAgent;
    private Func<Vector3> _destinationGetter;
    private AgentMoveType _moveType;
    private float _range;

    public MoveToAction(IAgent agent, Func<Vector3> destinationGetter, AgentMoveType moveType, float range) : base(null) 
    {
        _selfAgent = agent;
        _destinationGetter = destinationGetter;
        _moveType = moveType;
        _range = range;
    }

    public override INode.STATE Evaluate()
    {
        Vector3 destination = _destinationGetter();
        _selfAgent.MoveTo(destination, _moveType);

        if (_selfAgent.HasArrived(destination, _range))
        {
            _selfAgent.ResetMoveCommand();
            return INode.STATE.SUCCESS;
        }
        
        return INode.STATE.RUN;
    }
}