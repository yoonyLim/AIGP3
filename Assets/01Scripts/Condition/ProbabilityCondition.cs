using UnityEngine;

public class ProbabilityConditionNode : ConditionNode
{
    private float probability;

    public ProbabilityConditionNode(float probability)
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
