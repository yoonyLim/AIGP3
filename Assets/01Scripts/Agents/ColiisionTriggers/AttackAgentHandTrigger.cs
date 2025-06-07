using UnityEngine;

public class AttackAgentHandTrigger : MonoBehaviour
{
    AttackAgent root;

    private void Start()
    {
        root = GetComponentInParent<AttackAgent>();
    }
    private void OnTriggerEnter(Collider other)
    {
        root.OnHitByPunch(other);
    }
}
