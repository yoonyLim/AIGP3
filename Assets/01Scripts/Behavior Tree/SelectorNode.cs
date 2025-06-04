using System.Collections.Generic;
using System;

public class SelectorNode : INode
{
    List<INode> children; // ���� ��带 ���� �� �ֵ��� ����Ʈ ����
    private int _currentChildIndex = 0;

    public SelectorNode() { children = new List<INode>(); }

    public void Add(INode node) { children.Add(node); } // �����Ϳ� �ڽĳ�带 �߰��ϴ� �޼���

    public INode.STATE Evaluate()
    {
        while (_currentChildIndex < children.Count)
        {
            INode.STATE childState = children[_currentChildIndex].Evaluate();
            
            // child ����� state �� �ϳ��� SUCCESS �̸� ������ ��ȯ
            // ���� ���� ��� RUN ��ȯ
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
        // �ݺ����� �����ٸ� �ش� �������� �ڽĳ����� ���� FAILED �����̹Ƿ� �����ʹ� FAILED ��ȯ
        return INode.STATE.FAILED;
    }
}