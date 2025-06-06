using UnityEngine;

public class CanBlockCondition : ConditionNode
{
    private DefenseAgent self;
    private AttackAgent enemy;
    private bool canBlock;
    private bool wasEnemyAttacking = false;
    private bool isEnemyAttacking = false;


    public CanBlockCondition(IAgent self, IAgent enemy) : base(null)
    {
        this.self = self as DefenseAgent;
        this.enemy = enemy as AttackAgent;
    }

    public override INode.STATE Evaluate()
    {
        if (enemy == null || self == null)
            return INode.STATE.FAILED;

        isEnemyAttacking = enemy.IsAttacking;
        if (!wasEnemyAttacking && isEnemyAttacking)
        {
            canBlock = true;
        }
        wasEnemyAttacking = isEnemyAttacking;

        if (canBlock && isEnemyAttacking)
        {
            canBlock = false;
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.FAILED;
    }
}
