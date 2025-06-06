using System;
using UnityEngine;

public class DodgeOrDashAction : ActionNode
{
    private readonly IAgent _selfAgent;
    private readonly Func<Vector3> _destinationGetter;
    private readonly float _distance;
    private readonly float _force;
    private readonly float _duration;
    private readonly bool _shouldDash;

    private bool _hasStarted = false;
    private float _elapsedTime = 0f;
    private Vector3 _direction;
    private Quaternion _rotation;

    public DodgeOrDashAction(IAgent selfAgent, Func<Vector3> destinationGetter, float distance, float force, float duration, bool shouldDash)
        : base(null)
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
            _elapsedTime = 0f;

            Vector3 targetPos = _destinationGetter();

            if (_shouldDash)
            {
                _selfAgent.GetAgent().BeginDash(targetPos, out _direction); 
            }
            else
            {
                _selfAgent.GetAgent().BeginDodge(targetPos, _distance, out _direction, out _rotation);
            }
        }

        bool canMove = _shouldDash
            ? _selfAgent.GetAgent().TryDash(_direction, _force)
            : _selfAgent.GetAgent().TryDodge(_direction, _rotation, _force);

        if (!canMove)
        {
            Debug.Log("DodgeOrDash 실패: 충돌 감지");
            CleanUp();
            return INode.STATE.FAILED;
        }

        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= _duration)
        {
            CleanUp();
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.RUN;
    }

    private void CleanUp()
    {
        _hasStarted = false;
        _elapsedTime = 0f;
        _selfAgent.ResetMoveCommand();
    }
}