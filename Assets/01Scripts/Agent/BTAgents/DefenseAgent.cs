using System;
using System.Collections;
using UnityEngine;

public class DefenseAgent : BaseAgent
{
    private static readonly int BlockStart = Animator.StringToHash("BlockStart");
    private static readonly int Damage = Animator.StringToHash("Damage");
    private static readonly int BlockEnd = Animator.StringToHash("BlockEnd");
    private static readonly int Attack = Animator.StringToHash("Attack");
    public event Action OnBlockSucceeded;
    public event Action OnBlockFailed;

    private bool isBlocking = false;
    private bool hasBlockSucceeded = false;
    public bool HasBlockSucceeded => hasBlockSucceeded;

    [SerializeField] private float blockDuration = 1f;

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

    public void Block(Vector3 targetPos)
    {
        isBlocking = true;
        Vector3 dir = targetPos - transform.localPosition;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            rb.MoveRotation(rot);
        }

        _blockCooldown.Value = 0f;
        
        animator.SetTrigger(BlockStart);
        StartCoroutine(BlockCoroutine());
    }


    public override bool TakeDamage(float amount)
    {
        if (isBlocking)
        {
            isBlocking = false;
            hasBlockSucceeded = true;
            OnBlockSucceeded?.Invoke();
            
            return false;
        }
        
        hasBlockSucceeded = false;
        base.TakeDamage(amount);
        animator.SetTrigger(Damage);
        
        return true;
    }


    private IEnumerator BlockCoroutine()
    {
        yield return new WaitForSeconds(blockDuration);
        if (isBlocking)
        {
            isBlocking = false;
            animator.SetTrigger(BlockEnd);
            OnBlockFailed?.Invoke();
        }

    }

    private IEnumerator CounterAttackCoroutine()
    {
        _attackCooldown.Value = 0;
        
        punchHitBox.enabled = true;
        animator.SetTrigger(Attack);
        yield return new WaitForSeconds(0.5f);
        punchHitBox.enabled = false;
    }

    public void CounterAttack()
    {
        StartCoroutine(CounterAttackCoroutine());
    }

    public void OnHitByPunch(Collider other)
    {
        if (other.TryGetComponent(out AttackAgent target))
        {
            target.TakeDamage(GameManager.Instance.GetDAPunchDamage);
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
