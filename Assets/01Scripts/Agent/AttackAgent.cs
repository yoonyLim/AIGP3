using System;
using UnityEngine;

public class AttackAgent : BaseAgent
{
    public bool isAttacking;

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
    }

    private void Update()
    {
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }
}
