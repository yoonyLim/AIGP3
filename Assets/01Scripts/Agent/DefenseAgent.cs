using System;
using System.Collections;
using UnityEngine;

public class DefenseAgent : BaseAgent
{
    public event Action OnBlockSucceeded;
    public event Action OnBlockFailed;

    private bool isBlocking = false;
    private bool hasBlockSucceeded = false;

    [SerializeField] private float blockDuration = 1f;


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
        
        Debug.Log("BLOCK");
        animator.SetTrigger("BlockStart");
        StartCoroutine(BlockCoroutine());
    }


    public override void TakeDamage(float amount)
    {
        if (isBlocking)
        {
            hasBlockSucceeded = true;
            isBlocking = false;
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
}
