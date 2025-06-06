using System.Collections.Generic;
using System;

public class SequenceNode : INode
{
    List<INode> children; // 자식 노드들을 담을 수 있는 리스트
    private int _currentChildIndex = 0;

    public SequenceNode() { children = new List<INode>(); }

    public void Add(INode node) { children.Add(node); }

    public INode.STATE Evaluate()
    {
        // 자식 노드의 수가 0 이하라면 실패
        if (children.Count <= 0)
            return INode.STATE.FAILED;

        while (_currentChildIndex < children.Count)
        {
            INode.STATE childState = children[_currentChildIndex].Evaluate();
            
            // 자식 노드들중 하나라도 FAILED 라면 시퀀스는 FAILED
            switch (childState)
            {
                case INode.STATE.RUN:
                    return INode.STATE.RUN;
                // SUCCESS 이면 아래는 검사하지 않고 continue 키워드로 다시 반복문 호출
                case INode.STATE.SUCCESS:
                    _currentChildIndex++;
                    continue;
                case INode.STATE.FAILED:
                    _currentChildIndex = 0;
                    return INode.STATE.FAILED;
            }
        }

        _currentChildIndex = 0;
        // FAILED 에 걸리지 않고 반복문을 빠져나왔으므로 시퀀스는 SUCCESS
        return INode.STATE.SUCCESS;
    }
}

