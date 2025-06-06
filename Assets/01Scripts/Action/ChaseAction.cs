using System;
using UnityEngine;

public class ChaseAction : ActionNode
{
    private IAgent _selfAgent;
    private Func<Vector3> _destinationGetter;
    private AgentMoveType _moveType;
    private float _range;
    private LayerMask wallMask = LayerMask.GetMask("Wall");

    public ChaseAction(IAgent agent, Func<Vector3> destinationGetter, AgentMoveType moveType, float range) : base(null) 
    {
        _selfAgent = agent;
        _destinationGetter = destinationGetter;
        _moveType = moveType;
        _range = range;
    }

    public override INode.STATE Evaluate()
    {
        Vector3 destination = _destinationGetter();

        bool canMove = _selfAgent.TryMoveTo(destination, _moveType);
        if (!canMove)
        {
            Debug.Log("chase, 벽에 부딪혀서 종료합니다");
            _selfAgent.ResetMoveCommand();
            return INode.STATE.FAILED;    
        }

        if (_selfAgent.HasArrived(destination, _range))
        {
            _selfAgent.ResetMoveCommand();
            return INode.STATE.SUCCESS;
        }
        
        return INode.STATE.RUN;
    }
}