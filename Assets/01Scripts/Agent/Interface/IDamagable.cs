using System;

public interface IDamageable
{
    bool TakeDamage(float amount);
    void Die();

    Action OnDeath { get; set; }
}