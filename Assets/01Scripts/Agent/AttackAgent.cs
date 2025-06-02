using System;
using System.Collections;
using UnityEngine;


public class AttackAgent : BaseAgent
{
    [SerializeField] private Collider punchHitBox;
    [SerializeField] private Collider kickHitBox;

    private float strafeAngle = 0f;
    private float punchDuration = 0.5f;
    private float kickDuration = 1.0f;

    public event Action OnAttackSucceeded;
    public event Action OnAttackFailed;

    private bool punchHit = false;
    private bool kickHit = false;


    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
    }


    // Strafe
    public void StrafeAround(Vector3 centerPos, float radius = 3f, float angularSpeed = 90f, int direction = 1)
    {
        Vector3 toSelf = (transform.localPosition - centerPos).normalized;

        float angleDelta = angularSpeed * Time.deltaTime * direction;
        Quaternion rotation = Quaternion.AngleAxis(angleDelta, Vector3.up);
        Vector3 rotatedDir = rotation * toSelf;

        Vector3 nextPos = centerPos + rotatedDir * radius;
        Vector3 moveDir = (nextPos - transform.localPosition).normalized;

        float moveSpeed = GetMoveSpeed(AgentMoveType.Strafe);

        Vector3 lookDir = (centerPos - transform.localPosition).normalized;
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime));
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);
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

        kickHitBox.enabled = true;
        animator.SetTrigger("Attack2");
        yield return new WaitForSeconds(kickDuration);
        kickHitBox.enabled = false;

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
        }
    }

    public void OnHitByKick(Collider other)
    {
        if (other.TryGetComponent(out DefenseAgent target))
        {
            target.TakeDamage(10f);
            kickHit = true;
        }
    }


    private void Update()
    {
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }

}
