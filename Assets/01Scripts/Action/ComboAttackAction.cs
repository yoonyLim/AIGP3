using UnityEngine;

public class ComboAttackAction : ActionNode
{
    private AttackAgent attacker;
    private bool isStarted = false;
    private bool isFinished = false;
    private bool isSuccess = false;

    public ComboAttackAction(AttackAgent self) : base(null)
    {
        this.attacker = self;
    }

    public override INode.STATE Evaluate()
    {
        if (!isStarted)
        {
            Debug.Log("attack");
            isStarted = true;
            isFinished = false;
            isSuccess = false;

            attacker.OnAttackSucceeded += OnSuccess;
            attacker.OnAttackFailed += OnFailure;

            attacker.PlayCombo(); // ³»ºÎ¿¡¼­ ÆÝÄ¡ ¡æ µô·¹ÀÌ ¡æ Å± ÀÚµ¿ Àç»ý
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
