using UnityEngine;

public class KickAttackAction : ActionNode
{
    private AttackAgent attacker;

    private bool isStarted = false;
    private bool isFinished = false;
    private bool isSuccess = false;

    public KickAttackAction(AttackAgent self) : base(null)
    {
        this.attacker = self;
    }

    public override INode.STATE Evaluate()
    {
        if (!isStarted)
        {
            // Debug.Log("attack");
            isStarted = true;
            isFinished = false;
            isSuccess = false;

            attacker.OnAttackSucceeded += OnSuccess;
            attacker.OnAttackFailed += OnFailure;

            attacker.PlayPunch(); // ���ο��� ��ġ �� ������ �� ű �ڵ� ���
            return INode.STATE.RUN;
        }

        if (isFinished)
        {
            Cleanup();
            return isSuccess ? INode.STATE.SUCCESS : INode.STATE.FAILED;
        }

        return INode.STATE.RUN;
    }

    private void OnSuccess()
    {
        isFinished = true;
        isSuccess = true;
    }

    private void OnFailure()
    {
        isFinished = true;
        isSuccess = false;
    }

    private void Cleanup()
    {
        attacker.OnAttackSucceeded -= OnSuccess;
        attacker.OnAttackFailed -= OnFailure;
        isStarted = false;
    }
}
