using System;

public class ActionNode : INode
{
    public Func<INode.STATE> action; // ��ȯ���� INode.STATE �� �븮��

    public ActionNode(Func<INode.STATE> action) // ��带 ������ �� �Ű������� �븮�ڸ� ����(������)
    {
        this.action = action;
    }

    public INode.STATE Evaluate()
    {
        // �븮�ڰ� null �� �ƴ� �� ȣ��, null �� ��� Failed ��ȯ
        return action?.Invoke() ?? INode.STATE.FAILED;
    }
}