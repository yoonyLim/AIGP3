using System;
using System.Collections;
using UnityEngine;


public class AttackAgent : BaseAgent
{
    private static readonly int Damage = Animator.StringToHash("Damage");
    private static readonly int Attack1 = Animator.StringToHash("Attack1");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
    
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
    
    protected override void Start()
    {
        base.Start();
        
        punchHitBox.enabled = false;
        kickHitBox.enabled = false;
        
        // Get GameManager settings
        _dodgeCooldown.Value = GameManager.Instance.GetAADodgeCooldown;
        _attackCooldown.Value = GameManager.Instance.GetAAAttackCooldown;
    }

    public override void ResetStatus()
    {
        base.ResetStatus();
        
        punchHitBox.enabled = false;
        kickHitBox.enabled = false;
        wasTargetDamaged = false;
        IsAttacking = false;
        
        // Get GameManager settings
        _dodgeCooldown.Value = GameManager.Instance.GetAADodgeCooldown;
        _attackCooldown.Value = GameManager.Instance.GetAAAttackCooldown;
    }

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
            wasTargetDamaged = target.TakeDamage(GameManager.Instance.GetAAPunchDamage);
            punchHit = true;
            punchHitBox.enabled = false;
        }
    }

    public void OnHitByKick(Collider other)
    {
        if (other.TryGetComponent(out DefenseAgent target))
        {
            target.TakeDamage(GameManager.Instance.GetAAKickDamage);
            kickHit = true;
            kickHitBox.enabled = false;
        }
    }

    public bool GetCanComboAttack()
    {
        return wasTargetDamaged;
    }

    protected override void Update()
    {
        base.Update();

        if (_dodgeCooldown.Value < GameManager.Instance.GetAADodgeCooldown)
            _dodgeCooldown.Value = Mathf.Clamp(_dodgeCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetAADodgeCooldown);
        
        if (_attackCooldown.Value < GameManager.Instance.GetAAAttackCooldown)
            _attackCooldown.Value = Mathf.Clamp(_attackCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetAAAttackCooldown);
    }
}
