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
                    // Debug.DrawRay(_selfAgent.GetLocalPos(), hit.point, Color.red, 10f);
                    Debug.Log(hit.point);
                    Debug.Log(_selfAgent.GetLocalPos());
                    return INode.STATE.FAILED; // dash failed
                }
                _selfAgent.GetAgent().Dash(_dodgeDirection, _force);
            }
            else
            {
                bool success = _selfAgent.GetAgent().TryDodge(_destinationGetter(), _force, _distance);
                // Debug.Log("Dodge");

                if (!success)
                    return INode.STATE.FAILED;
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