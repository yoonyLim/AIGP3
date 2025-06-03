using System.Collections.Generic;
using System;

public class SelectorNode : INode
{
    List<INode> children; // 여러 노드를 가질 수 있도록 리스트 생성
    private int _currentChildIndex = 0;

    public SelectorNode() { children = new List<INode>(); }

    public void Add(INode node) { children.Add(node); } // 셀렉터에 자식노드를 추가하는 메서드

    public INode.STATE Evaluate()
    {
        while (_currentChildIndex < children.Count)
        {
            INode.STATE childState = children[_currentChildIndex].Evaluate();
            
            // child 노드의 state 가 하나라도 SUCCESS 이면 성공을 반환
            // 실행 중인 경우 RUN 반환
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
        // 반복문이 끝났다면 해당 셀렉터의 자식노드들은 전부 FAILED 상태이므로 셀렉터는 FAILED 반환
        return INode.STATE.FAILED;
    }
}