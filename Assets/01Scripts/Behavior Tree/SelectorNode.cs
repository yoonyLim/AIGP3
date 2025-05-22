using System.Collections.Generic;
using System;

public class SelectorNode : INode
{
    List<INode> children; // ���� ��带 ���� �� �ֵ��� ����Ʈ ����

    public SelectorNode() { children = new List<INode>(); }

    public void Add(INode node) { children.Add(node); } // �����Ϳ� �ڽĳ�带 �߰��ϴ� �޼���

    public INode.STATE Evaluate()
    {
        // ����Ʈ ���� ������ ���ʺ���(���� ������) �˻�
        foreach (INode child in children)
        {

            INode.STATE state = child.Evaluate();
            // child ����� state �� �ϳ��� SUCCESS �̸� ������ ��ȯ
            // ���� ���� ��� RUN ��ȯ
            switch (state)
            {
                case INode.STATE.SUCCESS:
                    return INode.STATE.SUCCESS;
                case INode.STATE.RUN:
                    return INode.STATE.RUN;
            }
        }
        // �ݺ����� �����ٸ� �ش� �������� �ڽĳ����� ���� FAILED �����̹Ƿ� �����ʹ� FAILED ��ȯ
        return INode.STATE.FAILED;
    }
}