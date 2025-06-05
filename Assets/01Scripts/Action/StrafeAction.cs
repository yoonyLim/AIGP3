using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class StrafeAction : ActionNode
{
    private readonly AttackAgent _self;
    private Func<Vector3> _destinationGetter;
    private readonly float _strafeDuration;
    private readonly float _strafeRadius;
    private readonly float _angularSpeed;

    private float _elapsedTime;
    private bool _hasStarted;
    private int _direction;

    public StrafeAction(IAgent self, Func<Vector3> destinationGetter, float probability, float radius = 3f, float duration = 5f, float angularSpeed = 90f) : base(null)
    {
        _self = self as AttackAgent;
        _destinationGetter = destinationGetter;
        _strafeRadius = radius;
        _strafeDuration = duration;
        _angularSpeed = angularSpeed;
    }

    public override INode.STATE Evaluate()
    {
        if (!_hasStarted)
        {
            _hasStarted = true;
            _direction = Random.value > 0.5f ? 1 : -1;
        }
        
        bool canMove = _self.TryStrafe(_destinationGetter(), _strafeRadius, _angularSpeed, _direction, out int usedDirection);
        if (!canMove)
        {
             Debug.Log("strafe, 벽에 부딪혀서 종료합니다");
             CleanUp();
             return INode.STATE.FAILED;
        }

        _elapsedTime +=  Time.deltaTime;
        _direction = usedDirection;

        if (_elapsedTime > _strafeDuration)
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
        _self.ResetMoveCommand();
    }
}
