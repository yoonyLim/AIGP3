using UnityEngine;

public class CanComboAttackCondition : ConditionNode
{
    private AttackAgent _selfAgent;


    public CanComboAttackCondition(IAgent selfAgent) : base(null)
    {
        _selfAgent = selfAgent as AttackAgent;
    }

    public override INode.STATE Evaluate()
    {
        if (!_selfAgent)
            return INode.STATE.FAILED;

        return _selfAgent.wasTargetDamaged ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}
