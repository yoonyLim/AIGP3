using System.Collections;
using UnityEditor.UI;
using UnityEngine;

public class AggressiveBT : MonoBehaviour
{
    private INode _root;
    private readonly Blackboard _blackboard = new Blackboard();

    [Header("Blackboard")] public float attackRange = 1f;
    public float attackCooldown = 2.5f;
    public float dodgeCooldown = 5f;
    public AttackAgent selfAgent;
    public DefenseAgent targetAgent;

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
        _blackboard.Set("attackRange", attackRange);
        _blackboard.Set("attackCooldown", attackCooldown);
        _blackboard.Set("canAttack", true);
        _blackboard.Set("dodgeCooldown", dodgeCooldown);
        _blackboard.Set("canDodge", true);
        _blackboard.Set("AttackTarget", targetAgent);
        _blackboard.Set("Self", selfAgent);

        // Dodge Sequence
        var dodge = new SequenceNode();
        dodge.Add(new ConditionNode(() => _blackboard.Get<bool>("canDodge")));
        dodge.Add(new ActionNode(() => 
            { 
                _blackboard.Set("canDodge", false);
                
                Vector3 forceToApply = transform.forward * 50f;
                rb.AddForce(forceToApply, ForceMode.Impulse);
                Debug.Log("Dodge");
                
                StartCoroutine(ResetBool("canDodge",  _blackboard.Get<float>("dodgeCooldown")));
                
                return INode.STATE.SUCCESS;
            }));

        // Attack Sequence
        var attack = new SequenceNode();
        attack.Add(new ConditionNode(() => Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition) < _blackboard.Get<float>("attackRange")));
        attack.Add(new ConditionNode(() => _blackboard.Get<bool>("canAttack")));
        attack.Add(new ActionNode(() => 
            { 
                _blackboard.Set("canAttack", false);
                selfAgent.isAttacking = true;
                
                _blackboard.Get<DefenseAgent>("AttackTarget").TakeDamage(10f);
                Debug.Log("Attack");
                
                StartCoroutine(ResetBool("canAttack",  _blackboard.Get<float>("attackCooldown")));
                
                return INode.STATE.SUCCESS;
            }));
        /*attak.Add(new ConditionNode(() => isPrevioudAttackSuccessful));
        attack.Add(new ActionNode((() =>
        {
            // Combo Attack
        }));*/

        // Chase Sequence
        var chase = new SequenceNode();
        chase.Add(new ConditionNode(() => Vector3.Distance(transform.localPosition, targetAgent.transform.localPosition) > _blackboard.Get<float>("attackRange"))); 
        chase.Add(new ActionNode(() =>
            {
                // Vector3 targetPos = targetAgent.GetPosition();
                _blackboard.Get<AttackAgent>("Self").MoveTo(targetAgent.transform.localPosition, AgentMoveType.Chase);
                
                return INode.STATE.RUN;
            }));

        // Sate Selecctor
        var selector = new SelectorNode();
        selector.Add(dodge);
        selector.Add(attack);
        selector.Add(chase);

        _root = selector;
    }

    void Update()
    {
        _root.Evaluate();
    }
}
