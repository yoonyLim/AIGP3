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
        
        _self.Strafe(_destinationGetter(), _strafeRadius, _angularSpeed, _direction);
        _elapsedTime +=  Time.deltaTime;

        if (_elapsedTime > _strafeDuration)
        {
            _hasStarted = false;
            _elapsedTime = 0f;
            _self.ResetMoveCommand();
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.RUN;
    }
}
