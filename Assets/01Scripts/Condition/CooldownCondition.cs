public class CooldownCondition : ConditionNode
{
    private readonly Blackboard blackboard;
    private readonly string key;

    public CooldownCondition(string key, Blackboard blackboard)
        : base(null)
    {
        this.key = key;
        this.blackboard = blackboard;
    }

    public override INode.STATE Evaluate()
    {
        return blackboard.Get<bool>(key) ? INode.STATE.SUCCESS : INode.STATE.FAILED;
    }
}
