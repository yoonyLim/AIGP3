using UnityEngine;

public class AttackAgentBlockAction : ActionNode
{
    private AttackAgent self;
    private IAgent target;

    private bool isStarted = false;
    private bool isFinished = false;
    private bool wasSuccess = false;


    public AttackAgentBlockAction(IAgent self, IAgent target) : base(null)
    {
        this.self = self as AttackAgent;
        this.target = target;
    }

    public override INode.STATE Evaluate()
    {
        if (!isStarted)
        {
            isStarted = true;
            self.OnBlockSucceeded += OnSuccess;
            self.OnBlockFailed += OnFailure;

            self.Block(target.GetLocalPos());
        }

        if (isFinished)
        {
            Cleanup();
            return wasSuccess ? INode.STATE.SUCCESS : INode.STATE.FAILED;
        }

        return INode.STATE.RUN;
    }

    private void OnSuccess()
    {
        isFinished = true;
        wasSuccess = true;
    }

    private void OnFailure()
    {
        isFinished = true;
        wasSuccess = false;
    }

    private void Cleanup()
    {
        self.OnBlockSucceeded -= OnSuccess;
        self.OnBlockFailed -= OnFailure;
        isStarted = false;
    }
}
