using UnityEngine;

public class BlockAction : ActionNode
{
    private DefenseAgent self;
    private IAgent target;

    private bool isStarted = false;
    private bool isFinished = false;
    private bool wasSuccess = false;


    public BlockAction(IAgent self, IAgent target) : base(null)
    {
        this.self = self as DefenseAgent;
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
