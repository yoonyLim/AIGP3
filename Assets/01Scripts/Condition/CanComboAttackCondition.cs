using UnityEngine;

public class CanComboAttackCondition : ConditionNode
{
    private DefenseAgent target;


    public CanComboAttackCondition(IAgent target) : base(null)
    {
        this.target = target as DefenseAgent;
    }

    public override INode.STATE Evaluate()
    {
        if (target == null)
            return INode.STATE.FAILED;

        return target.HasBlockSucceeded ? INode.STATE.FAILED : INode.STATE.SUCCESS;
    }
}
