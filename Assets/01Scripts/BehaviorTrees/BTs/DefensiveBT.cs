using System.Collections;
using UnityEditor.UI;
using UnityEngine;

public class DefensiveBT : MonoBehaviour
{
    private INode _root;
    private readonly Blackboard _blackboard = new Blackboard();
    private float _dodgeCooldown;
    private float _attackCooldown;
    private float _blockCooldown;
    private float _fleeCooldown;

    [Header("Probability")]
    [SerializeField] private float blockProbability = 0.5f;
    [SerializeField] private float counterAttackProbability = 0.7f;
    [SerializeField] private float fleeProbability = 0.3f;
    [SerializeField] private float dodgeProbability = 0.3f;
    [SerializeField] private float attackProbability = 0.2f;

    [Header("Property")] 
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float blockRange = 1f;
    [SerializeField] private float dodgeDistance = 2f;
    [SerializeField] private float dodgeForce = 0.5f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float fleeDistance = 5f;


    [SerializeField] private DefenseAgent selfAgent;
    [SerializeField] private AttackAgent targetAgent;
    
    void Start()
    {
        _dodgeCooldown = GameManager.Instance.GetDADodgeCooldown;
        _attackCooldown = GameManager.Instance.GetDAAttackCooldown;
        _blockCooldown = GameManager.Instance.GetDABlockCooldown;
        _fleeCooldown = GameManager.Instance.GetDAFleeCooldown;
        
        _blackboard.Set("attackRange", attackRange);
        _blackboard.Set("dodgeCooldown", _dodgeCooldown);
        _blackboard.Set("attackCooldown", _attackCooldown);
        _blackboard.Set("blockCooldown", _blockCooldown);
        _blackboard.Set("fleeCooldown", _fleeCooldown);

        _blackboard.Set("canDodge", true);
        _blackboard.Set("canAttack", true);
        _blackboard.Set("canBlock", true);
        _blackboard.Set("canFlee", true);

        // Block 
        var blockSequence = new SequenceNode();
        blockSequence.Add(new ProbabilityCondition(blockProbability));
        blockSequence.Add(new CanBlockCondition(selfAgent, targetAgent));
        blockSequence.Add(new CooldownCondition(selfAgent, "canBlock", _blackboard, _blackboard.Get<float>("blockCooldown")));
        blockSequence.Add(new BlockAction(selfAgent, targetAgent));
        
        var counterAttackSequence = new SequenceNode();
        counterAttackSequence.Add(new ProbabilityCondition(counterAttackProbability));
        counterAttackSequence.Add(new CanCounterAttackCondition(selfAgent));
        counterAttackSequence.Add(new CooldownCondition(selfAgent, "canAttack", _blackboard, _blackboard.Get<float>("attackCooldown")));
        counterAttackSequence.Add(new CounterAttackAction(selfAgent));

        // Dodge
        var dodgeSequence = new SequenceNode();
        dodgeSequence.Add(new ProbabilityCondition(dodgeProbability));
        dodgeSequence.Add(new CooldownCondition(selfAgent, "canDodge", _blackboard, _blackboard.Get<float>("dodgeCooldown")));
        dodgeSequence.Add(new DodgeOrDashAction(selfAgent, targetAgent.GetLocalPos, dodgeDistance, dodgeForce, dodgeDuration, false));
        dodgeSequence.Add(new RotateAction(selfAgent, targetAgent.GetLocalPos));

        // Flee
        var fleeSequence = new SequenceNode();
        fleeSequence.Add(new ProbabilityCondition(fleeProbability));
        fleeSequence.Add(new CooldownCondition(selfAgent, "canFlee", _blackboard, _blackboard.Get<float>("fleeCooldown")));
        fleeSequence.Add(new FleeAction(selfAgent, targetAgent.GetLocalPos, AgentMoveType.Flee, fleeDistance));
        fleeSequence.Add(new RotateAction(selfAgent, targetAgent.GetLocalPos));

        // Attack
        var attackSequecne = new SequenceNode();
        attackSequecne.Add(new ProbabilityCondition(attackProbability));
        attackSequecne.Add(new CooldownCondition(selfAgent, "canAttack", _blackboard, _blackboard.Get<float>("attackCooldown")));
        attackSequecne.Add(new ChaseAction(selfAgent, targetAgent.GetLocalPos, AgentMoveType.Chase, _blackboard.Get<float>("attackRange")));
        attackSequecne.Add(new CounterAttackAction(selfAgent));

        // In Block Range
        var inBlockRangeSelector = new SelectorNode();
        inBlockRangeSelector.Add(blockSequence);
        inBlockRangeSelector.Add(counterAttackSequence);
        inBlockRangeSelector.Add(dodgeSequence);
        inBlockRangeSelector.Add(fleeSequence);
        inBlockRangeSelector.Add(attackSequecne);

        // *************
        
        var outsideBlockRangeSelector = new SelectorNode();
        outsideBlockRangeSelector.Add(dodgeSequence);
        outsideBlockRangeSelector.Add(fleeSequence);

        // *************
        
        var inBlockRangeSequence = new SequenceNode();
        inBlockRangeSequence.Add(new TargetInRangeCondition(selfAgent, targetAgent, blockRange));
        inBlockRangeSequence.Add(inBlockRangeSelector);

        var outsideBlockRangeSequence = new SequenceNode();
        outsideBlockRangeSequence.Add(new TargetOutRangeCondition(selfAgent, targetAgent, blockRange));
        outsideBlockRangeSequence.Add(outsideBlockRangeSelector);

        // Root Selector
        var rootSelector = new SelectorNode();
        rootSelector.Add(inBlockRangeSequence);
        rootSelector.Add(outsideBlockRangeSequence);

        _root = rootSelector;
    }

    void Update()
    {
        if (!GameManager.Instance.IsEpisodeDone)
            _root.Evaluate();
    }
}
