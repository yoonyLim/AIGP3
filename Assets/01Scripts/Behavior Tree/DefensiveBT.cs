using System.Collections;
using UnityEditor.UI;
using UnityEngine;

public class DefenssiveBT : MonoBehaviour
{
    private INode _root;
    private readonly Blackboard _blackboard = new Blackboard();

    [Header("Probability")]
    [SerializeField] private float blockProbability = 0.5f;
    [SerializeField] private float counterAttackProbability = 0.5f;
    [SerializeField] private float fleeProbability = 0.3f;
    [SerializeField] private float dodgeProbability = 0.3f;
    [SerializeField] private float attackProbability = 0.2f;

    [Header("Cooldown")]
    [SerializeField] private float blockCooldown = 2.5f;
    [SerializeField] private float counterAttackCooldown = 2.5f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float dodgeCooldown = 5f;
    [SerializeField] private float fleeCooldown = 5f;

    [Header("Property")]
    [SerializeField] private float blockRange = 1f;
    [SerializeField] private float strafeRange = 3f;
    [SerializeField] private float dodgeDistance = 0.2f;
    [SerializeField] private float dodgeForce = 1f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float fleeDistance = 4f;


    public DefenseAgent selfAgent;
    public AttackAgent targetAgent;
    
    void Start()
    {
        _blackboard.Set("blockCooldown", blockCooldown);
        _blackboard.Set("counterAttackCooldown", counterAttackCooldown);
        _blackboard.Set("attackCooldown", attackCooldown);
        _blackboard.Set("dodgeCooldown", dodgeCooldown);
        _blackboard.Set("fleeCooldown", fleeCooldown);

        _blackboard.Set("canBlock", true);
        _blackboard.Set("canCounterAttack", true);
        _blackboard.Set("canAttack", true);
        _blackboard.Set("canDodge", true);
        _blackboard.Set("canFlee", true);



        // Block 
        var blockSequence = new SequenceNode();
        blockSequence.Add(new CanBlockCondition(selfAgent, targetAgent));
        blockSequence.Add(new CooldownCondition(selfAgent, "canBlock", _blackboard, _blackboard.Get<float>("blockCooldown")));
        blockSequence.Add(new ProbabilityCondition(blockProbability));
        blockSequence.Add(new BlockAction(selfAgent, targetAgent));
        blockSequence.Add(new CanCounterAttackCondition(selfAgent));
        blockSequence.Add(new CooldownCondition(selfAgent, "canCounterAttack", _blackboard, _blackboard.Get<float>("counterAttackCooldown")));
        blockSequence.Add(new ProbabilityCondition(counterAttackProbability));
        blockSequence.Add(new CounterAttackAction(selfAgent));

        // Dodge
        var dodgeSequence = new SequenceNode();
        dodgeSequence.Add(new CooldownCondition(selfAgent, "canDodge", _blackboard, _blackboard.Get<float>("dodgeCooldown")));
        dodgeSequence.Add(new ProbabilityCondition(dodgeProbability));
        dodgeSequence.Add(new DodgeOrDashAction(selfAgent, targetAgent.GetLocalPos, dodgeDistance, dodgeForce, dodgeDuration, false));

        // Flee
        var fleeSequence = new SequenceNode();
        fleeSequence.Add(new CooldownCondition(selfAgent, "canFlee", _blackboard, _blackboard.Get<float>("fleeCooldown")));
        blockSequence.Add(new ProbabilityCondition(fleeProbability));
        fleeSequence.Add(new FleeAction(selfAgent, targetAgent.GetLocalPos, AgentMoveType.Flee, fleeDistance));

        // Attack
        var attackSequecne = new SequenceNode();
        attackSequecne.Add(new CooldownCondition(selfAgent, "canAttack", _blackboard, _blackboard.Get<float>("attackCooldown")));
        blockSequence.Add(new ProbabilityCondition(attackProbability));
        blockSequence.Add(new CounterAttackAction(selfAgent));


        // In Block Range
        var inBlockRangeSelector = new SelectorNode();
        inBlockRangeSelector.Add(blockSequence);
        inBlockRangeSelector.Add(dodgeSequence);
        inBlockRangeSelector.Add(fleeSequence);
        inBlockRangeSelector.Add(attackSequecne);


        // *************

        var inStrafeRangeSelector = new SelectorNode();
        inStrafeRangeSelector.Add(dodgeSequence);
        inStrafeRangeSelector.Add(fleeSequence);


        // *************


        var inBlockRangeSequence = new SequenceNode();
        inBlockRangeSequence.Add(new TargetInRangeCondition(selfAgent, targetAgent, blockRange));
        inBlockRangeSequence.Add(inBlockRangeSelector);


        var inStrafeRangeSequence = new SequenceNode();
        inStrafeRangeSequence.Add(new TargetInRangeCondition(selfAgent, targetAgent, strafeRange));
        inStrafeRangeSequence.Add(inStrafeRangeSelector);


        // Root Selector
        var rootSelector = new SelectorNode();
        rootSelector.Add(inBlockRangeSelector);

        _root = rootSelector;
    }

    void Update()
    {
        _root.Evaluate();
    }
}
