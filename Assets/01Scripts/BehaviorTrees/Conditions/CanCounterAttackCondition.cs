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
        if (self == null)
            return INode.STATE.FAILED;
        
        Debug.Log("Can Counter Attack: " + self.HasBlockSucceeded);

        return self.HasBlockSucceeded ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}
