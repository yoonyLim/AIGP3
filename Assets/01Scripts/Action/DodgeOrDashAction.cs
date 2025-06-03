using System;
using UnityEngine;
using Random = System.Random;

public class DodgeOrDashAction : ActionNode
{
    private IAgent _agent;
    private Func<Vector3> _destinationGetter;
    private float _distance;
    private float _force;
    private float _duration;
    private bool _shouldDash;
    
    private bool _hasDodgeStarted = false;
    private Vector3 _dodgeDirection = Vector3.zero;
    private float _elapsedTime = 0f;

    public DodgeOrDashAction(IAgent agent, Func<Vector3> destinationGetter, float distance, float force, float duration, bool shouldDash) : base(null) 
    {
        _agent = agent;
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
            float dodgeAngle = 0f;

            if (_shouldDash)
            {
                _dodgeDirection = Quaternion.Euler(0, dodgeAngle, 0) * new Vector3((_destinationGetter() - _agent.GetLocalPos()).x, 0, (_destinationGetter() - _agent.GetLocalPos()).z).normalized;
                
                
                // check if dash is possible
                /*if (Physics.Raycast(_agent.GetLocalPos(), _dodgeDirection, out RaycastHit hit, _distance))
                {
                    Debug.Log("too close - failed");
                    return INode.STATE.FAILED; // dash failed
                }*/
            }
            else
            {
                dodgeAngle = GetRandomAngle(); // random direction
                _dodgeDirection = Quaternion.Euler(0, dodgeAngle, 0) * new Vector3((_agent.GetLocalPos() - _destinationGetter()).x, 0, (_agent.GetLocalPos() - _destinationGetter()).z).normalized;
                
                // get random dodge direction until no collision detected
                while (Physics.Raycast(_agent.GetLocalPos(), _dodgeDirection, out RaycastHit hit, _distance))
                {
                    dodgeAngle = GetRandomAngle();
                    _dodgeDirection = Quaternion.Euler(0, dodgeAngle, 0) * new Vector3((_agent.GetLocalPos() - _destinationGetter()).x, 0, (_agent.GetLocalPos() - _destinationGetter()).z).normalized;
                }
            }

            _hasDodgeStarted = true;
        }
        
        _agent.Dodge(_dodgeDirection * (_force * Time.deltaTime));
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _duration)
        {
            Debug.Log("DodgeOrDashAction done");
            _hasDodgeStarted = false;
            _elapsedTime = 0f;
            return INode.STATE.SUCCESS;       
        }
        
        return INode.STATE.RUN;
    }
}