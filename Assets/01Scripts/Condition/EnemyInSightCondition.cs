using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInSightCondition : ConditionNode
{
    private IAgent self;
    private IAgent enemy;
    private float detectionRadius;

    public EnemyInSightCondition(IAgent self, IAgent enemy, float detectionRadius = 5f) : base(null)
    {
        this.self = self;
        this.enemy = enemy;
        this.detectionRadius = detectionRadius;
    }

    public override INode.STATE Evaluate()
    {
        if (enemy == null) 
            return INode.STATE.FAILED;

        bool isEnemy = self.GetAgentType() != enemy.GetAgentType();
        float distance = Vector3.Distance(self.GetPosition(), enemy.GetPosition());

        if (isEnemy && distance <= detectionRadius)
        {
            return INode.STATE.SUCCESS;
        }
        return INode.STATE.FAILED;
    }
}
