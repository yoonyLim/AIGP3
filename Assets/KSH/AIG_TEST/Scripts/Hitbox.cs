using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float damage = 15f;
    // [SerializeField] private bool isPlayerOwned = true;
    Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;                  // �⺻�� ����
    }

    public void Enable() => col.enabled = true;   // �ִϸ��̼� �̺�Ʈ ����
    public void Disable() => col.enabled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hurtbox"))
        {
            var attacker = GetComponentInParent<Unity.MLAgents.Agent>();
            var defender = other.GetComponentInParent<Unity.MLAgents.Agent>();
            bool blocked = defender is DefenderAgent def && def.IsBlocking;
            CombatManager.ReportHit(attacker, defender, damage, blocked);
        }
    }
}
