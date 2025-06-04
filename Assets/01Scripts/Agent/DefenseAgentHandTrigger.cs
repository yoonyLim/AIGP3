using UnityEngine;

public class DefenseAgentHandTrigger : MonoBehaviour
{
    DefenseAgent root;

    private void Start()
    {
        root = GetComponentInParent<DefenseAgent>();
    }
    private void OnTriggerEnter(Collider other)
    {
        root.OnHitByPunch(other);
    }
}
