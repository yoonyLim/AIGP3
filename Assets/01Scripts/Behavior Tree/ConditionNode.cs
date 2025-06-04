using System;
using UnityEngine;

public class ConditionNode : INode
{
    private Func<bool> condition;

    public ConditionNode(Func<bool> condition)
    {
        this.condition = condition;
    }

    public virtual INode.STATE Evaluate()
    {
        return condition() ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}