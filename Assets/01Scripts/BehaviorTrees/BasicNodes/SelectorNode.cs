using System.Collections.Generic;
using System;

public class SelectorNode : INode
{
    List<INode> children;
    private int _currentChildIndex = 0;

    public SelectorNode() { children = new List<INode>(); }

    public void Add(INode node) { children.Add(node); }

    public INode.STATE Evaluate()
    {
        while (_currentChildIndex < children.Count)
        {
            INode.STATE childState = children[_currentChildIndex].Evaluate();
            
            switch (childState)
            {
                case INode.STATE.SUCCESS:
                    _currentChildIndex = 0;
                    return INode.STATE.SUCCESS;
                case INode.STATE.RUN:
                    return INode.STATE.RUN;
                case INode.STATE.FAILED:
                    _currentChildIndex++;
                    continue;
            }
        }
        
        _currentChildIndex = 0;
        return INode.STATE.FAILED;
    }
}