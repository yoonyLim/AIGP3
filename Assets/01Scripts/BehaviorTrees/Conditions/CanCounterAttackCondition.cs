using UnityEngine;

public class CanCounterAttackCondition : ConditionNode
{
    private DefenseAgent self;


    public CanCounterAttackCondition(IAgent self) : base(null)
    {
        this.self = self as DefenseAgent;
    }

    public override INode.STATE Evaluate()
    {
        if (!self)
            return INode.STATE.FAILED;

        return self.HasBlockSucceeded ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}
