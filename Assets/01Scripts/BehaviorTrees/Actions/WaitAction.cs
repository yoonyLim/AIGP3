using System;
using UnityEngine;


public class WaitAction : ActionNode
{
    private float _waitTime;
    private float _elapsedTime = 0f;


    public WaitAction(float waitTime) : base(null)
    {
        _waitTime = waitTime;
    }


    public override INode.STATE Evaluate()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _waitTime)
        {
            _elapsedTime = 0f;
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.RUN;
    }
}
