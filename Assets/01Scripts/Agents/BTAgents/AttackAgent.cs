using System;
using System.Collections;
using UnityEngine;


public class AttackAgent : BaseAgent
{
    private static readonly int BlockStart = Animator.StringToHash("BlockStart");
    private static readonly int Damage = Animator.StringToHash("Damage");
    private static readonly int BlockEnd = Animator.StringToHash("BlockEnd");
    private static readonly int Attack1 = Animator.StringToHash("Attack1");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
    
    public event Action OnAttackSucceeded;
    public event Action OnAttackFailed;
    public event Action OnBlockSucceeded;
    
    private float punchDuration = 0.5f;
    private float kickDuration = 1.0f;

    private bool punchHit = false;
    private bool kickHit = false;
    public bool wasTargetDamaged { get; set; }
    
    [SerializeField] private Collider punchHitBox;
    [SerializeField] private Collider kickHitBox;
    
    protected override void Start()
    {
        base.Start();
        
        wasTargetDamaged = false;
        punchHitBox.enabled = false;
        kickHitBox.enabled = false;
        
        // Get GameManager settings
        _dodgeCooldown.Value = GameManager.Instance.GetAADodgeCooldown;
        _attackCooldown.Value = GameManager.Instance.GetAAAttackCooldown;
        _blockCooldown.Value = GameManager.Instance.GetAABlockCooldown;
    }

    public override void ResetStatus()
    {
        base.ResetStatus();
        
        punchHitBox.enabled = false;
        kickHitBox.enabled = false;
        wasTargetDamaged = false;
        
        // Get GameManager settings
        _dodgeCooldown.Value = GameManager.Instance.GetAADodgeCooldown;
        _attackCooldown.Value = GameManager.Instance.GetAAAttackCooldown;
        _blockCooldown.Value = GameManager.Instance.GetAABlockCooldown;
    }

    public override bool TakeDamage(float amount)
    {
        if (IsBlocking)
        {
            numSuccessfulBlocks++; // for csv record
            IsBlocking = false;
            OnBlockSucceeded?.Invoke();
            
            return false;
        }
        
        base.TakeDamage(amount);
        animator.SetTrigger(Damage);
        
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
        numAttacks++; // for csv record
        _attackCooldown.Value = 0;
        
        punchHit = false;
        punchHitBox.enabled = true;
        animator.SetTrigger(Attack1);
        yield return new WaitForSeconds(punchDuration);
        punchHitBox.enabled = false;

        if (punchHit)
            OnAttackSucceeded?.Invoke();
        else
        {
            numFailedAttacks++; // for csv record
            OnAttackFailed?.Invoke();
        }

        punchHit = false;
        IsAttacking = false;
    }

    private IEnumerator KickRoutine()
    {
        _attackCooldown.Value = 0;
        
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
            
            if (wasTargetDamaged)
                numSuccessfulAttacks++; // for csv record
            else
                numFailedAttacks++; // for csv record
            
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

    protected override void Update()
    {
        base.Update();
        
        if (_dodgeCooldown.Value < GameManager.Instance.GetAADodgeCooldown)
            _dodgeCooldown.Value = Mathf.Clamp(_dodgeCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetAADodgeCooldown);
        
        if (_attackCooldown.Value < GameManager.Instance.GetAAAttackCooldown)
            _attackCooldown.Value = Mathf.Clamp(_attackCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetAAAttackCooldown);
        
        if (_blockCooldown.Value < GameManager.Instance.GetAABlockCooldown)
            _blockCooldown.Value = Mathf.Clamp(_blockCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetAABlockCooldown);
    }
}
