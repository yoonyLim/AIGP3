using System;
using System.Collections;
using UnityEngine;


public class AttackAgent : BaseAgent
{
    [SerializeField] private Collider punchHitBox;
    [SerializeField] private Collider kickHitBox;
    
    private float punchDuration = 0.5f;
    private float kickDuration = 1.0f;

    public event Action OnAttackSucceeded;
    public event Action OnAttackFailed;

    private bool punchHit = false;
    private bool kickHit = false;
    public bool IsAttacking { get; private set; }
    

    public override void TakeDamage(float amount)
    {
        animator.SetTrigger("Damage");
        base.TakeDamage(amount);
    }


    // Attack

    public void PlayPunch()
    {
        StartCoroutine(PunchRoutine());
        IsAttacking = true;
    }

    public void PlayKick()
    {
        StartCoroutine(KickRoutine());
        IsAttacking = true;
    }

    private IEnumerator PunchRoutine()
    {
        punchHit = false;
        punchHitBox.enabled = true;
        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(punchDuration);
        punchHitBox.enabled = false;

        if (punchHit)
            OnAttackSucceeded?.Invoke();
        else
            OnAttackFailed?.Invoke();

        IsAttacking = false;
    }

    private IEnumerator KickRoutine()
    {
        kickHit = false;
        kickHitBox.enabled = true;
        animator.SetTrigger("Attack2");
        yield return new WaitForSeconds(kickDuration);
        kickHitBox.enabled = false;

        if (kickHit)
            OnAttackSucceeded?.Invoke();
        else
            OnAttackFailed?.Invoke();

        IsAttacking = false;
    }


    public void OnHitByPunch(Collider other)
    {
        if (other.TryGetComponent(out DefenseAgent target))
        {
            target.TakeDamage(5f);
            punchHit = true;
            punchHitBox.enabled = false;
            kickHitBox.enabled = true;
        }
    }

    public void OnHitByKick(Collider other)
    {
        if (other.TryGetComponent(out DefenseAgent target))
        {
            target.TakeDamage(10f);
            kickHit = true;
            kickHitBox.enabled = false;
        }
    }

    protected override void Start()
    {
        base.Start();

        punchHitBox.enabled = false;
        kickHitBox.enabled = false;
    }

}
