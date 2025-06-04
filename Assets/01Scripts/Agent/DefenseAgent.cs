using UnityEngine;

public class DefenseAgent : BaseAgent
{
    private bool isDefending;

    public override void TakeDamage(float amount)
    {
        float finalDamage = isDefending ? amount * 0.5f : amount;
        base.TakeDamage(finalDamage);
    }
}
