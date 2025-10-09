using System.Collections;
using UnityEditor.UI;
using UnityEngine;

public class AggressiveBT : MonoBehaviour
{
    private INode _root;
    private readonly Blackboard _blackboard = new Blackboard();
    private float _dashCooldown;
    private float _attackCooldown;
    private float _blockCooldown;
    
    [Header("Probability")]
    [SerializeField] private float dashProbability = 0.7f;
    [SerializeField] private float strafeProbability = 0.3f;
    [SerializeField] private float blockProbability = 0.1f;

    [Header("Property")]
    [SerializeField] private float strafeRange = 1f;
    [SerializeField] private float strafeDuration = 0.5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float dashDistance = 1f;
    [SerializeField] private float dashForce = 8f;
    [SerializeField] private float dashDuration = 0.2f;
    
    [SerializeField] private AttackAgent selfAgent;
    [SerializeField] private DefenseAgent targetAgent;

    void Start()
    {
        _dashCooldown = GameManager.Instance.GetAADodgeCooldown;
        _attackCooldown = GameManager.Instance.GetAAAttackCooldown;
        _blockCooldown = GameManager.Instance.GetAABlockCooldown;
        
        // Set Blackboard
        _blackboard.Set("attackRange", attackRange);
        _blackboard.Set("attackCooldown", _attackCooldown);
        _blackboard.Set("canAttack", true);
        _blackboard.Set("dashCooldown", _dashCooldown);
        _blackboard.Set("dashDistance", dashDistance);
        _blackboard.Set("dashForce", dashForce);
        _blackboard.Set("dashDuration", dashDuration);
        _blackboard.Set("canDash", true);
        _blackboard.Set("canBlock", true);
        _blackboard.Set("blockCooldown", _blockCooldown);
        _blackboard.Set("AttackTarget", targetAgent);
        _blackboard.Set("strafeRange", strafeRange);
        _blackboard.Set("StrafeDuration", strafeDuration);
        _blackboard.Set("Self", selfAgent);

        // 1. Dash or Chase Selector
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
        
        // 2. Attack or Strafe Selector   
        var attackOrStrafeSelector = new SelectorNode();
        
        // Random Strafe Sequence
        var randomStrafeSequence = new SequenceNode();
        randomStrafeSequence.Add(new ProbabilityCondition(strafeProbability));
        randomStrafeSequence.Add(new StrafeAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("strafeRange"), _blackboard.Get<float>("StrafeDuration")));
        
        // Random Dash Sequence
        var randomDashSequence = new SequenceNode();
        randomDashSequence.Add(new ProbabilityCondition(dashProbability));
        randomDashSequence.Add(new CooldownCondition(selfAgent, "canDash", _blackboard, _blackboard.Get<float>("dashCooldown")));
        randomDashSequence.Add(new DodgeOrDashAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("dashDistance"), _blackboard.Get<float>("dashForce"), _blackboard.Get<float>("dashDuration"), true));
        
        // Chase and Attack Sequence
        var chaseAndAttackSequence = new SequenceNode();
        chaseAndAttackSequence.Add(new CooldownCondition(selfAgent, "canAttack", _blackboard, _blackboard.Get<float>("attackCooldown")));
        chaseAndAttackSequence.Add(new ChaseAction(selfAgent, targetAgent.GetLocalPos, AgentMoveType.Chase, _blackboard.Get<float>("attackRange")));
        chaseAndAttackSequence.Add(new RotateAction(selfAgent, targetAgent.GetLocalPos));
        chaseAndAttackSequence.Add(new PunchAttackAction(selfAgent));
        chaseAndAttackSequence.Add(new CanComboAttackCondition(selfAgent));
        chaseAndAttackSequence.Add(new KickAttackAction(selfAgent));
        
        // Strafe Action
        var strafeAction = new StrafeAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("strafeRange"), _blackboard.Get<float>("StrafeDuration"));
        
        attackOrStrafeSelector.Add(randomStrafeSequence);
        attackOrStrafeSelector.Add(randomDashSequence);
        attackOrStrafeSelector.Add(chaseAndAttackSequence);
        attackOrStrafeSelector.Add(strafeAction);
        
        // Block Sequence
        var blockSequence = new SequenceNode();
        blockSequence.Add(new ProbabilityCondition(blockProbability));
        blockSequence.Add(new CooldownCondition(selfAgent, "canBlock", _blackboard, _blackboard.Get<float>("blockCooldown")));
        blockSequence.Add(new AttackAgentBlockAction(selfAgent, targetAgent));

        // Root Selector
        var rootSelector = new SelectorNode();
        rootSelector.Add(dashOrChaseSequence);
        rootSelector.Add(blockSequence);
        rootSelector.Add(attackOrStrafeSelector);

        _root = rootSelector;
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsEpisodeDone)
            _root.Evaluate();
        else
        {
            selfAgent.ResetMoveCommand();
            
            if (!selfAgent.HasWrittenCSV)
                selfAgent.WriteCSV("BTOffensive", !selfAgent.IsDead);
        }
    }
}
