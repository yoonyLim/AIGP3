using UnityEngine;

public class ProbabilityCondition : ConditionNode
{
    private float probability;

    public ProbabilityCondition(float probability)
        : base(null)
    {
        this.probability = probability;
    }

    public override INode.STATE Evaluate()
    {
        float rand = UnityEngine.Random.value;
        //Debug.Log($"[ProbNode] Random = {rand}");
        return rand < probability ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}
