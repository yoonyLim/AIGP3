using UnityEngine;

public class CounterAttackAction : ActionNode
{
    private DefenseAgent defender;

    public CounterAttackAction(DefenseAgent self) : base(null)
    {
        this.defender = self;
    }

    public override INode.STATE Evaluate()
    {
        if (defender == null)
            return INode.STATE.FAILED;

        defender.CounterAttack();
        return INode.STATE.SUCCESS;
    }
}
