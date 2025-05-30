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
            Debug.Log("Move To Node ����");
            currentNode = new MoveToNode(testAgent, moveTarget.position, AgentMoveType.Chase);
        }

        if (currentNode != null)
        {
            var result = currentNode.Evaluate();
            switch (result)
            {
                case INode.STATE.SUCCESS:
                case INode.STATE.FAILED:
                    Debug.Log(result == INode.STATE.SUCCESS ? "���� �Ϸ�" : "�̵� ����");
                    currentNode = null;
                    break;
                case INode.STATE.RUN:
                    Debug.Log("�̵� ��");
                    break;
            }
        }
    } 
}
