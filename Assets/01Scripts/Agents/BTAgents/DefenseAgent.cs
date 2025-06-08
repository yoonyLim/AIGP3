using System;
using System.Collections;
using UnityEngine;

public class DefenseAgent : BaseAgent
{
    private static readonly int Damage = Animator.StringToHash("Damage");
    private static readonly int BlockEnd = Animator.StringToHash("BlockEnd");
    private static readonly int Attack = Animator.StringToHash("Attack");
    
    public event Action OnBlockSucceeded;
    public event Action OnCounterAttackSucceeded;
    public event Action OnCounterAttackFailed;
    
    private bool hasBlockSucceeded = false;
    public bool HasBlockSucceeded => hasBlockSucceeded;
    private bool hasCounterAttackSucceeded = false;

    [SerializeField] private Collider punchHitBox;

    protected override void Start()
    {
        base.Start();
        
        punchHitBox.enabled = false;        
        
        // Get GameManager settings
        _dodgeCooldown.Value = GameManager.Instance.GetDADodgeCooldown;
        _attackCooldown.Value = GameManager.Instance.GetDAAttackCooldown;
        _blockCooldown.Value = GameManager.Instance.GetDABlockCooldown;
    }

    public override void ResetStatus()
    {
        base.ResetStatus();
        
        punchHitBox.enabled = false;
        hasBlockSucceeded = false;
        
        // Get GameManager settings
        _dodgeCooldown.Value = GameManager.Instance.GetDADodgeCooldown;
        _attackCooldown.Value = GameManager.Instance.GetDAAttackCooldown;
        _blockCooldown.Value = GameManager.Instance.GetDABlockCooldown;
    }
    
    public override bool TakeDamage(float amount)
    {
        if (IsBlocking)
        {
            IsBlocking = false;
            hasBlockSucceeded = true;
            OnBlockSucceeded?.Invoke();
            
            return false;
        }
        
        hasBlockSucceeded = false;
        base.TakeDamage(amount);
        animator.SetTrigger(Damage);
        
        return true;
    }

    private IEnumerator CounterAttackCoroutine()
    {
        _attackCooldown.Value = 0;
        
        punchHitBox.enabled = true;
        animator.SetTrigger(Attack);
        yield return new WaitForSeconds(0.5f);
        punchHitBox.enabled = false;
        
        if (!hasCounterAttackSucceeded)
            OnCounterAttackFailed?.Invoke();

        hasCounterAttackSucceeded = false;
    }

    public void CounterAttack()
    {
        StartCoroutine(CounterAttackCoroutine());
    }

    public void OnHitByPunch(Collider other)
    {
        if (other.TryGetComponent(out AttackAgent target))
        {
            OnCounterAttackSucceeded?.Invoke();
            target.TakeDamage(GameManager.Instance.GetDAPunchDamage);
            hasCounterAttackSucceeded = true;
            punchHitBox.enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (_dodgeCooldown.Value < GameManager.Instance.GetDADodgeCooldown)
            _dodgeCooldown.Value = Mathf.Clamp(_dodgeCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetDADodgeCooldown);
        
        if (_attackCooldown.Value < GameManager.Instance.GetDAAttackCooldown)
            _attackCooldown.Value = Mathf.Clamp(_attackCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetDAAttackCooldown);
        
        if (_blockCooldown.Value < GameManager.Instance.GetDABlockCooldown)
            _blockCooldown.Value = Mathf.Clamp(_blockCooldown.Value + Time.deltaTime, 0f, GameManager.Instance.GetDABlockCooldown);
    }
}
