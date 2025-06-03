using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
//using Unity.AI.Navigation;           // NavMeshAgent

//[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class AttackerAgent : Agent
{
    int attacksThrown;
    int attacksLanded;
    [Header("Refs")]
    [SerializeField] private Transform target;      // ��ǥ �Ǵ� ���
    [SerializeField] private Renderer groundRenderer;
    [SerializeField] private Color groundWin = Color.green;
    [SerializeField] private Color groundLose = Color.red;
    private Color groundDefault;

    //UnityEngine.AI.NavMeshAgent nav;
    Animator anim;
    Health hp;

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 1.5f;

    // ���� ��ٿ�
    private float nextAttackTime;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float turnSpeed = 360f;   // deg/s
    [SerializeField] private float avoidRadius = 0.5f;   // 장애물 회피용

    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;
    public bool ReadyToAttack() => Time.time >= nextAttackTime;

    // -------------------- Unity --------------------
    public override void Initialize()
    {
        Debug.Log("Initialize()");
        //nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = GetComponent<Animator>();
        hp = GetComponent<Health>();

        //nav.speed = walkSpeed;
        groundDefault = groundRenderer.material.color;
        attacksThrown = attacksLanded = 0;

        CurrentEpisode = 0;
        CumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");
        // ��ġ �� HP ����
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        target.localPosition = new Vector3(Random.Range(-3f, 3f), 0f,
                                             Random.Range(-3f, 3f));
        hp.ResetHp();
        nextAttackTime = Time.time;
        groundRenderer.material.color = groundDefault;
        attacksThrown = 0;
        attacksLanded = 0;

        CurrentEpisode++;
        CumulativeReward = 0f;

    }

    // -------------------- Observations --------------------
    public override void CollectObservations(VectorSensor sensor)
    {
        // ��� ��ġ ����
        Vector3 rel = target.position - transform.position;
        sensor.AddObservation(rel / 5f);                 // 3 floats
        sensor.AddObservation(transform.InverseTransformDirection(rel.normalized));

        // ü��
        sensor.AddObservation(hp.Value / hp.Max);
        var oppHp = target.GetComponent<Health>();
        sensor.AddObservation(oppHp ? oppHp.Value / oppHp.Max : 1f);

        // ��ٿ�
        sensor.AddObservation(Mathf.Clamp01(
            (nextAttackTime - Time.time) / attackCooldown));



        //bool oppAtk = target.GetComponent<Animator>()
        //               .GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        //sensor.AddObservation(oppAtk ? 1f : 0f);
    }

    // -------------------- Actions --------------------

    void ManualMove()
    {
        if (target == null) return;

        // 1) 목표 방향 계산
        Vector3 dir = (target.position - transform.position);
        dir.y = 0;                                      // 평면 이동
        float dist = dir.magnitude;

        if (dist > 0.05f)
        {
            dir.Normalize();

            /* 1-A) 회전 */
            Quaternion rotGoal = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, rotGoal, turnSpeed * Time.deltaTime);

            /* 1-B) 장애물 간단 회피 (Raycast) */
            if (Physics.SphereCast(transform.position + Vector3.up * 0.2f,
                                   0.2f, transform.forward,
                                   out RaycastHit hit, avoidRadius,
                                   LayerMask.GetMask("Wall")))
            {
                // 우측으로 살짝 비켜 걷기
                Vector3 side = Vector3.Cross(Vector3.up, hit.normal).normalized;
                transform.position += side * walkSpeed * 0.5f * Time.deltaTime;
            }
            else
            {
                /* 1-C) 전진 이동 */
                transform.position += transform.forward * walkSpeed * Time.deltaTime;
            }
            anim.SetBool("Walking", true);
        }
        else
        {
            anim.SetBool("Walking", false);
        }
    }
    public override void OnActionReceived(ActionBuffers act)
    {
        int attack = act.DiscreteActions[0];
        int evade = act.DiscreteActions[1];

        // 0. �׺���̼�
        //nav.SetDestination(target.position);
        //anim.SetBool("Walking", nav.velocity.magnitude > 0.05f);

        ManualMove();
        // 1. ȸ�� / ����
        switch (evade)
        {
            case 1: anim.SetTrigger("DodgeL"); break;
            case 2: anim.SetTrigger("DodgeR"); break;
            case 3: anim.SetTrigger("Block"); break;
        }

        // 2. ����
        if (ReadyToAttack())
        {
            if (attack == 1) anim.SetTrigger("Punch");
            else if (attack == 2) anim.SetTrigger("Kick");
            if (attack != 0) nextAttackTime = Time.time + attackCooldown;
            if (attack == 1 || attack == 2)
                attacksThrown++;
        }

        // �ð� �г�Ƽ
        AddReward(-0.002f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        d[0] = 0; d[1] = 0;

        if (Input.GetKey(KeyCode.Space)) d[0] = 1;           // ��ġ
        if (Input.GetKey(KeyCode.LeftShift)) d[0] = 2;       // ű
        if (Input.GetKey(KeyCode.Q)) d[1] = 1;               // ���� ȸ��
        if (Input.GetKey(KeyCode.E)) d[1] = 2;               // ������ ȸ��
        if (Input.GetKey(KeyCode.F)) d[1] = 3;               // ����
    }

    // -------------------- Reward hooks --------------------
    public void OnHitLanded(float dmg)
    {
        attacksLanded++;
        AddReward(dmg >= 10 ? 1.0f : 0.3f);
        FlashGround(groundWin);
        Debug.Log($"[Ep {CompletedEpisodes}] ★ 명중! 누적 {attacksLanded}/{attacksThrown}");
    }
    public void OnGotHit(float dmg)
    {
        Debug.Log($"[Ep {CompletedEpisodes}] ☆ 피격! HP {hp.Value}/{hp.Max}");
        AddReward(-0.25f);
        FlashGround(groundLose);
    }

    // ������ ���� Blink
    void FlashGround(Color c)
    {
        if (groundRenderer)
        {
            groundRenderer.material.color = c;
            Invoke(nameof(ResetGround), 0.15f);
        }
    }
    void ResetGround()
    {
        if (groundRenderer)
            groundRenderer.material.color = groundDefault;
    }
}
