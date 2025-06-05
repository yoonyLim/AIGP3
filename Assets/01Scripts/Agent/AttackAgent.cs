using System;
using System.Collections;
using UnityEngine;


public class AttackAgent : BaseAgent
{
    private static readonly int Damage = Animator.StringToHash("Damage");
    private static readonly int Attack1 = Animator.StringToHash("Attack1");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");

    [Header("UI Observer")]
    [SerializeField] private GenericObserver<float> _attackCooldown = new GenericObserver<float>(2.5f);
    private const float _maxAttackCooldown = 2.5f;
    
    [SerializeField] private Collider punchHitBox;
    [SerializeField] private Collider kickHitBox;
    
    private float punchDuration = 0.5f;
    private float kickDuration = 1.0f;

    public event Action OnAttackSucceeded;
    public event Action OnAttackFailed;

    private bool punchHit = false;
    private bool kickHit = false;
    private bool wasTargetDamaged = false;
    public bool IsAttacking { get; private set; }
    

    public override bool TakeDamage(float amount)
    {
        animator.SetTrigger(Damage);
        base.TakeDamage(amount);

        return true;
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
        _attackCooldown.Value = 0;
        
        punchHit = false;
        punchHitBox.enabled = true;
        animator.SetTrigger(Attack1);
        yield return new WaitForSeconds(punchDuration);
        punchHitBox.enabled = false;

        if (punchHit)
            OnAttackSucceeded?.Invoke();
        else
            OnAttackFailed?.Invoke();

        punchHit = false;
        IsAttacking = false;
    }

    private IEnumerator KickRoutine()
    {
        kickHit = false;
        kickHitBox.enabled = true;
        animator.SetTrigger(Attack2);
        yield return new WaitForSeconds(kickDuration);
        kickHitBox.enabled = false;

        if (kickHit)
            OnAttackSucceeded?.Invoke();
        else
            OnAttackFailed?.Invoke();

        wasTargetDamaged = false;
        kickHit = false;
        IsAttacking = false;
    }


    public void OnHitByPunch(Collider other)
    {
        if (other.TryGetComponent(out DefenseAgent target))
        {
            wasTargetDamaged = target.TakeDamage(5f);
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

    public bool GetCanComboAttack()
    {
        return wasTargetDamaged;
    }

    protected override void Start()
    {
        base.Start();
        
        punchHitBox.enabled = false;
        kickHitBox.enabled = false;
        
        _attackCooldown.Invoke();
    }

    protected override void Update()
    {
        base.Update();

        if (_attackCooldown.Value < _maxAttackCooldown)
            _attackCooldown.Value = Mathf.Clamp(_attackCooldown.Value + Time.deltaTime, 0f, _maxAttackCooldown);
    }
}
