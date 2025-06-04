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

        if (blocked) damage *= 0.2f;          // 80 % 피해 감소
        hp.Damage(damage);

        // 보상 지급 (공격/피격 각각)
        if (attacker is AttackerAgent atk)
            atk.OnHitLanded(damage);

        if (defender is AttackerAgent def)
            def.OnGotHit(damage);

        // 에피소드 종료 조건
        if (hp.IsDead)
        {
            attacker.AddReward(2.0f);
            defender.AddReward(-2.0f);
            Debug.Log("Episode 종료");
            attacker.EndEpisode();
            defender.EndEpisode();
        }
    }
}
