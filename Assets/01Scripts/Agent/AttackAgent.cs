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
    
    public override void TakeDamage(float amount)
    {
        animator.SetTrigger("Damage");
        base.TakeDamage(amount);
    }


    // Attack
    public void PlayCombo()
    {
        StartCoroutine(ComboRoutine());
    }

    private IEnumerator ComboRoutine()
    {
        punchHit = false;
        kickHit = false;

        punchHitBox.enabled = true;
        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(punchDuration);
        punchHitBox.enabled = false;

        if (punchHit)
        {
            kickHitBox.enabled = true;
            animator.SetTrigger("Attack2");
            yield return new WaitForSeconds(kickDuration);
            kickHitBox.enabled = false;
        }

        if (punchHit || kickHit)
            OnAttackSucceeded?.Invoke();
        else
            OnAttackFailed?.Invoke();
    }

    public void OnHitByPunch(Collider other)
    {
        if (other.TryGetComponent(out DefenseAgent target))
        {
            target.TakeDamage(5f);
            punchHit = true;
            punchHitBox.enabled = false;
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

    void Start()
    {
        punchHitBox.enabled = false;
        kickHitBox.enabled = false;
    }

    private void Update()
    {
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }

}
