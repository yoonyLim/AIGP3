using UnityEngine;

public class AttackAgentFootTrigger : MonoBehaviour
{
    AttackAgent root;

    private void Start()
    {
        root = GetComponentInParent<AttackAgent>();
    }
    private void OnTriggerEnter(Collider other)
    {
        root.OnHitByKick(other);
    }
}
