using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Health))]
public class DefenderAgent : Agent
{
    [SerializeField] private Transform attacker;
    [SerializeField] private bool ruleBased = true;    // 학습 전용일 땐 false
    Animator anim;
    Health hp;

    public bool IsBlocking { get; private set; }

    public override void Initialize()
    {
        anim = GetComponent<Animator>();
        hp = GetComponent<Health>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(2f, 0, 0);
        transform.localRotation = Quaternion.identity;
        hp.ResetHp();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 rel = attacker.position - transform.position;
        sensor.AddObservation(rel / 5f);
        sensor.AddObservation(hp.Value / hp.Max);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (ruleBased)
        {
            // 간단 Rule: 일정 확률로 블록
            if (!IsBlocking && Random.value < 0.1f)
            {
                anim.SetTrigger("Block");
                IsBlocking = true;
                Invoke(nameof(EndBlock), 0.8f);
            }
        }
        else
        {
            // 학습용: 0=none,1=block
            int act = actions.DiscreteActions[0];
            if (act == 1 && !IsBlocking)
            {
                anim.SetTrigger("Block");
                IsBlocking = true;
                Invoke(nameof(EndBlock), 0.8f);
            }
        }
    }
    void EndBlock() => IsBlocking = false;
}
