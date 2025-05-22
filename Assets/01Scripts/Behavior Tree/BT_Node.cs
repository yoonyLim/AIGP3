public interface INode
{
    public enum STATE
    { RUN, SUCCESS, FAILED }

    public INode.STATE Evaluate(); // 판단하여 상태 리턴
}