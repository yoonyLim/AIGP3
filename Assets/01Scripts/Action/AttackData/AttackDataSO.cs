using UnityEngine;

[CreateAssetMenu(fileName = "AttackSO", menuName = "Scriptable Objects/AttackSO")]
public class AttackDataSO : ScriptableObject
{
    public float power;
    public float cooldown;
}
