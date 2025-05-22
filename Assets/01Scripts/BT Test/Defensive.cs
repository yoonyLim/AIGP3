using UnityEngine;

public class Defensive : MonoBehaviour
{
    private INode root;
    private Blackboard blackboard = new Blackboard();

    public Transform player;
    public float hp = 100f;
    public float attackRange = 2.5f;
    public float attackCooldown = 0f;
    public float defenseCooldown = 0f;
    public float dodgeCooldown = 0f;
    public Aggressive target;

    void Start()
    {

        target = GameObject.FindObjectOfType<Aggressive>();

        // ���� ���
        blackboard.Set("hp", hp);
        blackboard.Set("attackCooldown", attackCooldown);
        blackboard.Set("defenseCooldown", defenseCooldown);
        blackboard.Set("dodgeCooldown", dodgeCooldown);
        blackboard.Set("target", target);
        blackboard.Set("self", this.transform);

        // ȸ��
        var dodge = new SequenceNode();
        dodge.Add(new ConditionNode(() => hp < 30f));
        dodge.Add(new ConditionNode(() => dodgeCooldown <= 0f));
        dodge.Add(new ActionNode(() =>
            {
                Debug.Log("dodge"); dodgeCooldown = 5.0f; //�����Լ��� �߰��ϴ� �κ�
                return INode.STATE.SUCCESS;
            }));

        // ����
        var attack = new SequenceNode();
        attack.Add(new ConditionNode(() => Vector3.Distance(transform.position, target.transform.position) < attackRange));
        attack.Add(new ConditionNode(() => attackCooldown <= 0f));
        attack.Add(new ActionNode(() =>
            {
                Debug.Log("attack"); attackCooldown = 2.5f; //�����Լ��� �߰��ϴ� �κ�
                return INode.STATE.SUCCESS;
            }));

        // ���
        var defend = new SequenceNode();
        defend.Add(new ConditionNode(() => Vector3.Distance(transform.position, target.transform.position) < attackRange + 1f));
        defend.Add(new ConditionNode(() => defenseCooldown <= 0f));
        defend.Add(new ActionNode(() =>
            {
                Debug.Log("defense"); defenseCooldown = 2.5f;//�����Լ��� �߰��ϴ� �κ�
                return INode.STATE.SUCCESS;
            }));

        // ����
        var chase = new ActionNode(() =>
        {
           // Debug.Log("chasing");
            //�����Լ��� �߰��ϴ� �κ�
            return INode.STATE.RUN;
        });

        // ��ü Ʈ��
        var selector = new SelectorNode();
        selector.Add(dodge);
        selector.Add(attack);
        selector.Add(defend);
        selector.Add(chase);

        root = selector;
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
        defenseCooldown -= Time.deltaTime;
        dodgeCooldown -= Time.deltaTime;

        root.Evaluate();
    }
}