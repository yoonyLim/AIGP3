using System.Collections;
using UnityEngine;

public class CooldownCondition : ConditionNode
{
    private IAgent _agent;
    private readonly Blackboard _blackboard;
    private readonly string _key;
    private readonly float _cooldownTime = 0f;
    
    private float _elapsedTime = 0f;

    public CooldownCondition(IAgent agent, string key, Blackboard blackboard, float cooldownTime) : base(null)
    {
        _agent = agent;
        _key = key;
        _blackboard = blackboard;
        _cooldownTime = cooldownTime;
    }

    public override INode.STATE Evaluate()
    {
        if (_blackboard.Get<bool>(_key))
        {
            _blackboard.Set<bool>(_key, false); // disable for cooldown time
            _agent.ResetCooldown(_key, _blackboard, _cooldownTime);
            return INode.STATE.SUCCESS;
        }
        
        return INode.STATE.FAILED;
    }
}
