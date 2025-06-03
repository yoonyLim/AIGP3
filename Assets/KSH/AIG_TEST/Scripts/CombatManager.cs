using UnityEngine;
using Unity.MLAgents;
using UnityEngine.InputSystem;

public static class CombatManager
{
    public static void ReportHit(Agent attacker, Agent defender,
                                 float damage, bool blocked)
    {
        var hp = defender.GetComponent<Health>();
        if (hp == null) return;

        if (blocked) damage *= 0.2f;          // 80 % ���� ����
        hp.Damage(damage);

        // ���� ���� (����/�ǰ� ����)
        if (attacker is AttackerAgent atk)
            atk.OnHitLanded(damage);

        if (defender is AttackerAgent def)
            def.OnGotHit(damage);

        // ���Ǽҵ� ���� ����
        if (hp.IsDead)
        {
            attacker.AddReward(2.0f);
            defender.AddReward(-2.0f);
            Debug.Log("Episode ����");
            attacker.EndEpisode();
            defender.EndEpisode();
        }
    }
}
