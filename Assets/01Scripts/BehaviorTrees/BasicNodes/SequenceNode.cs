using System.Collections.Generic;
using System;

public class SequenceNode : INode
{
    List<INode> children; // �ڽ� ������ ���� �� �ִ� ����Ʈ
    private int _currentChildIndex = 0;

    public SequenceNode() { children = new List<INode>(); }

    public void Add(INode node) { children.Add(node); }

    public INode.STATE Evaluate()
    {
        // �ڽ� ����� ���� 0 ���϶�� ����
        if (children.Count <= 0)
            return INode.STATE.FAILED;

        while (_currentChildIndex < children.Count)
        {
            INode.STATE childState = children[_currentChildIndex].Evaluate();
            
            // �ڽ� ������ �ϳ��� FAILED ��� �������� FAILED
            switch (childState)
            {
                case INode.STATE.RUN:
                    return INode.STATE.RUN;
                // SUCCESS �̸� �Ʒ��� �˻����� �ʰ� continue Ű����� �ٽ� �ݺ��� ȣ��
                case INode.STATE.SUCCESS:
                    _currentChildIndex++;
                    continue;
                case INode.STATE.FAILED:
                    _currentChildIndex = 0;
                    return INode.STATE.FAILED;
            }
        }

        _currentChildIndex = 0;
        // FAILED �� �ɸ��� �ʰ� �ݺ����� �����������Ƿ� �������� SUCCESS
        return INode.STATE.SUCCESS;
    }
}

