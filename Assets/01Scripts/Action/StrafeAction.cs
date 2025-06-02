using UnityEngine;

public class StrafeAction : ActionNode
{
    private readonly AttackAgent self;
    private readonly IAgent target;
    private readonly float strafeDuration;
    private readonly float strafeRadius;
    private readonly float angularSpeed;

    private float startTime;
    private bool isStarted;
    private int direction;

    public StrafeAction(IAgent self, IAgent target, float probability, float radius = 3f, float duration = 5f, float angularSpeed = 90f) : base(null)
    {
        this.self = self as AttackAgent;
        this.target = target;
        this.strafeRadius = radius;
        this.strafeDuration = duration;
        this.angularSpeed = angularSpeed;
    }

    public override INode.STATE Evaluate()
    {
        if (!isStarted)
        {
            isStarted = true;
            startTime = Time.time;
            direction = Random.value > 0.5f ? 1 : -1; // CW or CCW
            Debug.Log("start strafe");
        }

        if (Time.time - startTime > strafeDuration)
        {
            isStarted = false;
            Debug.Log("end strafe");
            return INode.STATE.SUCCESS;
        }

        // target 기준으로 strafing
        self.StrafeAround(target.GetLocalPos(), strafeRadius, angularSpeed * direction);

        return INode.STATE.RUN;
    }
}
