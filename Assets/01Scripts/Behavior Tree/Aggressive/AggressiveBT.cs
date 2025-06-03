using System.Collections;
using UnityEditor.UI;
using UnityEngine;

public class AggressiveBT : MonoBehaviour
{
    private INode _root;
    private readonly Blackboard _blackboard = new Blackboard();
    // public Blackboard Blackboard => _blackboard;

    [Header("Blackboard")]
    public float strafeRange = 3f;
    public float attackRange = 1f;
    public float attackCooldown = 5f;
    public float dashCooldown = 5f;
    public float dashDistance = 3f;
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
        // for rigid body
        Rigidbody rb = GetComponent<Rigidbody>();

        // Set Blackboard
        _blackboard.Set("strafeRange", strafeRange);
        _blackboard.Set("attackRange", attackRange);
        _blackboard.Set("attackCooldown", attackCooldown);
        _blackboard.Set("canAttack", true);
        _blackboard.Set("dashCooldown", dashCooldown);
        _blackboard.Set("dashDistance", dashDistance);
        _blackboard.Set("canDash", true);
        _blackboard.Set("AttackTarget", targetAgent);
        _blackboard.Set("Self", selfAgent);

        // Dodge Sequence
        //var dodge = new SequenceNode();
        //dodge.Add(new ConditionNode(() => _blackboard.Get<bool>("canDodge")));
        //dodge.Add(new ActionNode(() => 
        //    { 
        //        _blackboard.Set("canDodge", false);

        //        Vector3 forceToApply = transform.forward * 50f;
        //        rb.AddForce(forceToApply, ForceMode.Impulse);
        //        Debug.Log("Dodge");

        //        StartCoroutine(ResetBool("canDodge",  _blackboard.Get<float>("dodgeCooldown")));

        //        return INode.STATE.SUCCESS;
        //    }));


        
        // Strafe sequence
        var strafe = new SequenceNode();
        strafe.Add(new TargetInRangeCondition(selfAgent, targetAgent, _blackboard.Get<float>("strafeRange")));
        strafe.Add(new ProbabilityConditionNode(0.3f));
        strafe.Add(new StrafeAction(selfAgent, targetAgent, _blackboard.Get<float>("strafeRange")));


        // Chase or Attack Sequence
        var attack = new SequenceNode();
        //attack.Add(new TargetOutRangeCondition(selfAgent, targetAgent, _blackboard.Get<float>("attackRange")));
        //attack.Add(new MoveToAction(selfAgent, targetAgent.GetLocalPos(), AgentMoveType.Chase));

        attack.Add(new TargetInRangeCondition(selfAgent, targetAgent, _blackboard.Get<float>("strafeRange")));
        attack.Add(new CooldownCondition(selfAgent, "canAttack", _blackboard, _blackboard.Get<float>("attackCooldown")));
        attack.Add(new ComboAttackAction(selfAgent));
        attack.Add(new StartCooldownAction(this, _blackboard, "canAttack", attackCooldown));

       // 2. Attack or Strafe Selector   
        var attackOrStrafe = new SelectorNode();
        attackOrStrafe.Add(strafe);
        attackOrStrafe.Add(attack);

        // 2. Dash or Chase Sequence
        var dashOrChase = new SelectorNode();

        var dash = new SequenceNode();
        dash.Add(new ProbabilityConditionNode(dashProbability));
        dash.Add(new CooldownCondition(selfAgent, "canDash", _blackboard, _blackboard.Get<float>("dashCooldown")));
        dash.Add(new DodgeOrDashAction(selfAgent, targetAgent.GetLocalPos, _blackboard.Get<float>("dashDistance"), 10f, 0.2f, true));
        
        var chase = new SequenceNode();
        chase.Add(new TargetOutRangeCondition(selfAgent, targetAgent, _blackboard.Get<float>("strafeRange")));
        chase.Add(new MoveToAction(selfAgent, targetAgent.GetLocalPos, AgentMoveType.Chase));
        
        dashOrChase.Add(dash);
        dashOrChase.Add(chase);

        // State Selecctor
        var rootSelector = new SelectorNode();
        rootSelector.Add(dashOrChase);

        _root = rootSelector;
    }

    void Update()
    {
        _root.Evaluate();
    }
}
