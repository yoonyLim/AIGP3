using System;

public interface IDamageable
{
    void TakeDamage(float amount);
    void Die();

    Action OnDeath { get; set; }
}