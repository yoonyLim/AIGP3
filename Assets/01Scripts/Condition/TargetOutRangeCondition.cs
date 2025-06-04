using System;
using System.Collections.Generic;
using UnityEngine;

public class TargetOutRangeCondition : ConditionNode
{
    private IAgent _selfAgent;
    private IAgent _targetAgent;
    private float _range;

    public TargetOutRangeCondition(IAgent self, IAgent enemy, float range) : base(null)
    {
        _selfAgent = self;
        _targetAgent = enemy;
        _range = range;
    }

    public override INode.STATE Evaluate()
    {
        if (_targetAgent == null) 
            return INode.STATE.FAILED;

        bool isEnemy = _selfAgent.GetAgentType() != _targetAgent.GetAgentType();
        float distance = Vector3.Distance(_selfAgent.GetLocalPos(), _targetAgent.GetLocalPos());

        if (isEnemy && distance > _range)
            return INode.STATE.SUCCESS;
        
        return INode.STATE.FAILED;
    }
}
