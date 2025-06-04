using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHp = 100f;
    public float Max => maxHp;
    public float Value { get; private set; }

    private void Awake() => ResetHp();
    public void ResetHp() => Value = maxHp;

    public void Damage(float dmg)
    {
        Value = Mathf.Max(0f, Value - dmg);
    }

    public bool IsDead => Value <= 0f;
}
