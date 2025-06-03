using UnityEngine;

public class ActionTest : MonoBehaviour
{
    [SerializeField] BaseAgent testAgent;
    [SerializeField] BaseAgent enemyAgent;

    [SerializeField] Transform moveTarget;

    INode currentNode;


    void Start()
    {
        
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("�� ������ �߰�, ������ ����");

            var selector = new SelectorNode();

            var condition = new TargetInRangeCondition(testAgent, enemyAgent, 5f);
            var chaseNode = new MoveToAction(testAgent, enemyAgent.GetLocalPos, AgentMoveType.Chase);

            var chaseSequence = new SequenceNode();
            chaseSequence.Add(condition);
            chaseSequence.Add(chaseNode);

            var patrolNode = new RandomPatrolAction(testAgent, AgentMoveType.Patrol);

            selector.Add(chaseSequence);
            selector.Add(patrolNode);

            currentNode = selector;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("����");
            //var attackNode = new AttackAction(testAgent, enemyAgent, attackData);
            //currentNode = attackNode;
        }



            if (currentNode != null)
        {
            var result = currentNode.Evaluate();
            switch (result)
            {
                case INode.STATE.SUCCESS:
                case INode.STATE.FAILED:
                    Debug.Log(result == INode.STATE.SUCCESS ? "�ൿ �Ϸ�" : "�ൿ ����");
                    currentNode = null;
                    break;
                case INode.STATE.RUN:
                    break;
            }
        }
    } 
}
