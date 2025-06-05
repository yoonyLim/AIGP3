using System.Collections;
using UnityEditor.UI;
using UnityEngine;

public class AggressiveBT : MonoBehaviour
{
    private INode _root;
    private readonly Blackboard _blackboard = new Blackboard();


    [Header("Blackboard")]
    public float strafeRange = 3f;
    public float attackRange = 1.3f;
    public float attackCooldown = 2f;
    public float dashCooldown = 5f;
    public float dashDistance = 1f;
    public float dashForce = 8f;
    public float dashDuration = 0.2f;
    public AttackAgent selfAgent;
    public DefenseAgent targetAgent;
    
    [Header("BT Variables")]
    public float dashProbability = 0.7f;
    public float strafeProbability = 0.3f;

    private Coroutine _dodgeCoroutine;
    private Coroutine _attackCoroutine;
    
    private IEnumerator ResetBool(string val, float duration)
    {
        yield return new WaitForSeconds(duration);
        _blackboard.Set(val, true);
    }

    void Start()
    {
        // Set Blackboard
        _blackboard.Set("strafeRange", strafeRange);
        _blackboard.Set("attackRange", attackRange);
        _blackboard.Set("attackCooldown", attackCooldown);
        _blackboard.Set("canAttack", true);
        _blackboard.Set("dashCooldown", dashCooldown);
        _blackboard.Set("dashDistance", dashDistance);
        _blackboard.Set("dashForce", dashForce);
        _blackboard.Set("dashDuration", dashDuration);
        _blackboard.Set("canDash", true);
        _blackboard.Set("AttackTarget", targetAgent);
        _blackboard.Set("Self", selfAgent);

        // 2. Dash or Chase Selector
        var dashOrChaseSequence = new SequenceNode();
        
        var dashOrChaseDecorator = new SelectorNode();

        // Dash Sequence
        var dashSequence = new SequenceNode();
        dashSequence.Add(new ProbabilityCondition(dashProbability));
        dashSequence.Add(new CooldownCondition(selfAgent, "canDash", _blackboard, _blackboard.Get<float>("dashCooldown")));
        dashSequence.Add(new DodgeOrDashAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("dashDistance"), _blackboard.Get<float>("dashForce"), _blackboard.Get<float>("dashDuration"), true));
        
        // Chase Action
        var chaseAction = new ChaseAction(selfAgent, targetAgent.GetLocalPos, AgentMoveType.Chase, _blackboard.Get<float>("strafeRange"));
        
        dashOrChaseDecorator.Add(dashSequence);
        dashOrChaseDecorator.Add(chaseAction);
        
        dashOrChaseSequence.Add(new TargetOutRangeCondition(selfAgent, targetAgent, _blackboard.Get<float>("strafeRange") + 1f));
        dashOrChaseSequence.Add(dashOrChaseDecorator);
        
        // 3. Attack or Strafe Selector   
        var attackOrStrafeSelector = new SelectorNode();
        
        // Random Strafe Sequence
        var randomStrafeSequence = new SequenceNode();
        randomStrafeSequence.Add(new ProbabilityCondition(strafeProbability));
        randomStrafeSequence.Add(new StrafeAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("strafeRange")));
        
        // Random Dash Sequence
        var randomDashSequence = new SequenceNode();
        randomDashSequence.Add(new ProbabilityCondition(dashProbability - 0.3f));
        randomDashSequence.Add(new CooldownCondition(selfAgent, "canDash", _blackboard, _blackboard.Get<float>("dashCooldown")));
        randomDashSequence.Add(new DodgeOrDashAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("dashDistance"), _blackboard.Get<float>("dashForce"), _blackboard.Get<float>("dashDuration"), true));
        
        // Chase and Attack Sequence
        var chaseAndAttackSequence = new SequenceNode();
        chaseAndAttackSequence.Add(new CooldownCondition(selfAgent, "canAttack", _blackboard, _blackboard.Get<float>("attackCooldown")));
        chaseAndAttackSequence.Add(new ChaseAction(selfAgent, targetAgent.GetLocalPos, AgentMoveType.Chase, _blackboard.Get<float>("attackRange")));
        chaseAndAttackSequence.Add(new PunchAttackAction(selfAgent));
        chaseAndAttackSequence.Add(new CanComboAttackCondition(selfAgent));
        chaseAndAttackSequence.Add(new KickAttackAction(selfAgent));
        
        // Strafe Action
        var strafeAction = new StrafeAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("strafeRange"));
        
        attackOrStrafeSelector.Add(randomStrafeSequence);
        attackOrStrafeSelector.Add(randomDashSequence);
        attackOrStrafeSelector.Add(chaseAndAttackSequence);
        attackOrStrafeSelector.Add(strafeAction);

        // Root Selector
        var rootSelector = new SelectorNode();
        rootSelector.Add(dashOrChaseSequence);
        rootSelector.Add(attackOrStrafeSelector);

        _root = rootSelector;
    }

    void FixedUpdate()
    {
        _root.Evaluate();
    }
}
