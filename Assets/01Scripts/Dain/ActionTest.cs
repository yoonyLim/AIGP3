using UnityEngine;

public class ActionTest : MonoBehaviour
{
    [SerializeField] BaseAgent testAgent;
    [SerializeField] Transform moveTarget;
    INode currentNode;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("Move To Node 시작");
            currentNode = new MoveToNode(testAgent, moveTarget.position, AgentMoveType.Chase);
        }

        if (currentNode != null)
        {
            var result = currentNode.Evaluate();
            switch (result)
            {
                case INode.STATE.SUCCESS:
                case INode.STATE.FAILED:
                    Debug.Log(result == INode.STATE.SUCCESS ? "도착 완료" : "이동 실패");
                    currentNode = null;
                    break;
                case INode.STATE.RUN:
                    Debug.Log("이동 중");
                    break;
            }
        }
    } 
}
