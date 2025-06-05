using System;
using UnityEngine;
using Random = System.Random;
public class DodgeOrDashAction : ActionNode
{
    private IAgent _selfAgent;
    private Func<Vector3> _destinationGetter;
    private float _distance;
    private float _force;
    private float _duration;
    private bool _shouldDash;

    private bool _hasStarted = false;
    private float _elapsedTime = 0f;

    public DodgeOrDashAction(IAgent selfAgent, Func<Vector3> destinationGetter, float distance, float force, float duration, bool shouldDash) : base(null)
    {
        _selfAgent = selfAgent;
        _destinationGetter = destinationGetter;
        _distance = distance;
        _force = force;
        _duration = duration;
        _shouldDash = shouldDash;
    }

    public override INode.STATE Evaluate()
    {
        if (!_hasStarted)
        {
            _hasStarted = true;

            bool success = _shouldDash
                ? _selfAgent.GetAgent().TryDash(_destinationGetter(), _force, _distance)
                : _selfAgent.GetAgent().TryDodge(_destinationGetter(), _force, _distance);

            if (!success)
            {
                Reset();
                return INode.STATE.FAILED;
            }
        }

        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _duration)
        {
            Reset();
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.RUN;
    }

    private void Reset()
    {
        _hasStarted = false;
        _elapsedTime = 0f;
        _selfAgent.ResetMoveCommand();
    }
}
