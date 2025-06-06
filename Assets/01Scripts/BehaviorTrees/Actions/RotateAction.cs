using System;
using UnityEngine;


public class RotateAction : ActionNode
{
    private IAgent _selfAgent;
    private Func<Vector3> _targetPositionGetter; 

    public RotateAction(IAgent agent, Func<Vector3> targetPositionGetter) : base(null)
    {
        _selfAgent = agent;
        _targetPositionGetter = targetPositionGetter;
    }

    private bool AreQuaternionsNearlyEqual(Quaternion q1, Quaternion q2)
    {
        float dotProduct = Quaternion.Dot(q1, q2);
        return Mathf.Abs(dotProduct) >= 1 - 0.01f;
    }
    
    public override INode.STATE Evaluate()
    {
        Vector3 lookDir = (_targetPositionGetter() - _selfAgent.GetLocalPos()).normalized;
        Quaternion lookAtTargetRot = Quaternion.LookRotation(lookDir);

        _selfAgent.RotateTo(lookAtTargetRot);
        
        /*Debug.Log("look at rot: " + lookAtTargetRot);
        Debug.Log("self rot: " + _selfAgent.GetLocalRot());*/

        if (AreQuaternionsNearlyEqual(lookAtTargetRot, _selfAgent.GetLocalRot()))
        {
            _selfAgent.ResetMoveCommand();
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.RUN;
    }
}
