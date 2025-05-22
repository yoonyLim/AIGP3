using System;

public class ConditionNode : INode
{
    private Func<bool> condition;

    public ConditionNode(Func<bool> condition)
    {
        this.condition = condition;
    }

    public INode.STATE Evaluate()
    {
        return condition() ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}