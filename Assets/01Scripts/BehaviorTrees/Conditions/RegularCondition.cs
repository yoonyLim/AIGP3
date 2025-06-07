using System.Collections;
using UnityEngine;

public class RegularCondition : ConditionNode
{
    private bool _condition;

    public RegularCondition(bool condition) : base(null)
    {
        _condition = condition;
    }

    public override INode.STATE Evaluate()
    {
        Debug.Log("no combo attack: " + _condition);
        return _condition ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}
