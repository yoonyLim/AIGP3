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
        //Debug.Log("counter attack");
        if (!defender)
            return INode.STATE.FAILED;

        defender.CounterAttack();
        return INode.STATE.SUCCESS;
    }
}
