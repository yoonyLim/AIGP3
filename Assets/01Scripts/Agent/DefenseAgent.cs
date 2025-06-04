using System;
using System.Collections;
using UnityEngine;

public class DefenseAgent : BaseAgent
{
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
        
        animator.SetTrigger("BlockStart");
        StartCoroutine(BlockCoroutine());
    }


    public override void TakeDamage(float amount)
    {
        if (isBlocking)
        {
            hasBlockSucceeded = true;
            OnBlockSucceeded?.Invoke();
            return;
        }

        base.TakeDamage(amount);
        animator.SetTrigger("Damage");
    }


    private IEnumerator BlockCoroutine()
    {
        yield return new WaitForSeconds(blockDuration);
        if (isBlocking)
        {
            isBlocking = false;
            animator.SetTrigger("BlockEnd");
            OnBlockFailed?.Invoke();
        }

    }

    public void CounterAttack()
    {
        punchHitBox.enabled = true;
        animator.SetTrigger("Attack");

    }

    public void OnHitByPunch(Collider other)
    {
        if (other.TryGetComponent(out AttackAgent target))
        {
            target.TakeDamage(10f);
            punchHitBox.enabled = false;
        }
    }
}
