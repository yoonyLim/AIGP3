public interface INode
{
    public enum STATE
    { RUN, SUCCESS, FAILED }

    public INode.STATE Evaluate(); // �Ǵ��Ͽ� ���� ����
}