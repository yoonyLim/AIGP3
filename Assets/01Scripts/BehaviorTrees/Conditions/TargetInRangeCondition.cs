using System;
using System.Collections.Generic;
using UnityEngine;

public class TargetInRangeCondition : ConditionNode
{
    private IAgent self;
    private IAgent enemy;
    private float range;

    public TargetInRangeCondition(IAgent self, IAgent enemy, float range) : base(null)
    {
        this.self = self;
        this.enemy = enemy;
        this.range = range;
    }

    public override INode.STATE Evaluate()
    {
        if (enemy == null) 
            return INode.STATE.FAILED;

        bool isEnemy = self.GetAgentType() != enemy.GetAgentType();
        float distance = Vector3.Distance(self.GetLocalPos(), enemy.GetLocalPos());

        if (isEnemy && distance <= range)
        {
            return INode.STATE.SUCCESS;
        }
        return INode.STATE.FAILED;
    }
}
