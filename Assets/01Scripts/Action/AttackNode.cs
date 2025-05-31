using UnityEngine;

public class AttackNode : ActionNode
{
    private IAgent self;
    private IDamageable target;
    private AttackDataSO attackData;
    private float lastAttackTime;


    public AttackNode(IAgent self, IDamageable target, AttackDataSO attackData) : base(null)
    {
        this.self = self;
        this.target = target;
        this.attackData = attackData;
        this.lastAttackTime = -Mathf.Infinity;  // 처음에 무조건 공격하도록
    }

    public override INode.STATE Evaluate()
    {
        if (target == null)
            return INode.STATE.FAILED;

        if (Time.time - lastAttackTime >= attackData.cooldown)
        {
            target.TakeDamage(attackData.power);
            lastAttackTime = Time.time;
            Debug.Log("ATTACK!");
        }

        return INode.STATE.RUN;  
    }
}
