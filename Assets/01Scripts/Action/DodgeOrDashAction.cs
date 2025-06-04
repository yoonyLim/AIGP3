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
    
    private bool _hasDodgeStarted = false;
    private Vector3 _dodgeDirection = Vector3.zero;
    private float _elapsedTime = 0f;

    private int tries = 0;
    private int maxTries = 50;

    public DodgeOrDashAction(IAgent selfAgent, Func<Vector3> destinationGetter, float distance, float force, float duration, bool shouldDash) : base(null) 
    {
        _selfAgent = selfAgent;
        _destinationGetter = destinationGetter;
        _distance = distance;
        _force = force;
        _duration = duration;
        _shouldDash = shouldDash;
    }

    float GetRandomAngle()
    {
        // random angle in range [90, 180]
        // multiplied by random direction
        return (UnityEngine.Random.value * 90f + 90f) * (UnityEngine.Random.value < 0.5f ? 1f : -1f);
    }

    public override INode.STATE Evaluate()
    {
        if (!_hasDodgeStarted)
        {
            _hasDodgeStarted = true;
            float dodgeAngle = 0f;

            if (_shouldDash)
            {
                _dodgeDirection = Quaternion.Euler(0, dodgeAngle, 0) * new Vector3((_destinationGetter() - _selfAgent.GetLocalPos()).x, 0, (_destinationGetter() - _selfAgent.GetLocalPos()).z).normalized;
                
                // check if dash is possible
                if (Physics.Raycast(_selfAgent.GetLocalPos(), _dodgeDirection, out RaycastHit hit, _distance))
                {
                    Debug.DrawRay(_selfAgent.GetLocalPos(), hit.point, Color.red, 10f);
                    Debug.Log(hit.point);
                    Debug.Log(_selfAgent.GetLocalPos());
                    return INode.STATE.FAILED; // dash failed
                }
                _selfAgent.Dodge(_dodgeDirection, _force, DodgeType.Dash);
            }
            else
            {
                dodgeAngle = GetRandomAngle(); // random direction
                _dodgeDirection = Quaternion.Euler(0, dodgeAngle, 0) * new Vector3((_selfAgent.GetLocalPos() - _destinationGetter()).x, 0, (_selfAgent.GetLocalPos() - _destinationGetter()).z).normalized;
                
                // get random dodge direction until no collision detected
                while (Physics.Raycast(_selfAgent.GetWorldPos(), _dodgeDirection, out RaycastHit hit, _distance) && tries < maxTries)
                {
                    dodgeAngle = GetRandomAngle();
                    _dodgeDirection = Quaternion.Euler(0, dodgeAngle, 0) * new Vector3((_selfAgent.GetLocalPos() - _destinationGetter()).x, 0, (_selfAgent.GetLocalPos() - _destinationGetter()).z).normalized;
                    
                    tries++;
                    if (tries >= maxTries)
                    {
                        return INode.STATE.FAILED;
                    }

                }

                _selfAgent.Dodge(_dodgeDirection, _force, DodgeType.Dodge);
            }
        }
        
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _duration)
        {
            _hasDodgeStarted = false;
            _elapsedTime = 0f;
            _selfAgent.ResetMoveCommand();
            return INode.STATE.SUCCESS;       
        }
        
        return INode.STATE.RUN;
    }
}