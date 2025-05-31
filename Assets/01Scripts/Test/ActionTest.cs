using UnityEngine;

public class ActionTest : MonoBehaviour
{
    [SerializeField] BaseAgent testAgent;
    [SerializeField] BaseAgent enemyAgent;

    [SerializeField] Transform moveTarget;
    [SerializeField] AttackDataSO attackData;

    INode currentNode;


    void Start()
    {
        
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("적 있으면 추격, 없으면 순찰");

            var selector = new SelectorNode();

            var condition = new EnemyInSightCondition(testAgent, enemyAgent, 5f);
            var chaseNode = new MoveToNode(testAgent, enemyAgent.transform.position, AgentMoveType.Chase);

            var chaseSequence = new SequenceNode();
            chaseSequence.Add(condition);
            chaseSequence.Add(chaseNode);

            var patrolNode = new RandomPatrolNode(testAgent, AgentMoveType.Patrol);

            selector.Add(chaseSequence);
            selector.Add(patrolNode);

            currentNode = selector;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("공격");
            var attackNode = new AttackNode(testAgent, enemyAgent, attackData);
            currentNode = attackNode;
        }



            if (currentNode != null)
        {
            var result = currentNode.Evaluate();
            switch (result)
            {
                case INode.STATE.SUCCESS:
                case INode.STATE.FAILED:
                    Debug.Log(result == INode.STATE.SUCCESS ? "행동 완료" : "행동 실패");
                    currentNode = null;
                    break;
                case INode.STATE.RUN:
                    break;
            }
        }
    } 
}
