using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    // For all to access
    private bool _isEpisodeDone = false;
    
    // Attack agent settings
    [Header("Attack Agent Cooldowns")]
    [SerializeField] private float _AA_dodgeCooldown = 5f; // initially stated 5 int he project instructions
    [SerializeField] private float _AA_attackCooldown = 2f; // initially stated 2.5 int he project instructions
    
    [Header("Attack Agent Properties")]
    [SerializeField] private float _AA_punchDamage = 5f;
    [SerializeField] private float _AA_kickDamage = 9f;
    
    // Defense agent settings
    [Header("Defense Agent Cooldowns")]
    [SerializeField] private float _DA_dodgeCooldown = 5f; // initially stated 5 int he project instructions
    [SerializeField] private float _DA_attackCooldown = 2f; // initially stated 2.5 int he project instructions
    [SerializeField] private float _DA_blockCooldown = 2.5f; // initially stated 2.5 int he project instructions
    [SerializeField] private float _DA_fleeCooldown = 5f;
    
    [Header("Defense Agent Properties")]
    [SerializeField] private float _DA_punchDamage = 10f;

    #region Getters
    // Getters
    public bool IsEpisodeDone
    {
        get => _isEpisodeDone;
        set => _isEpisodeDone = value;
    }
    
    public float GetAADodgeCooldown => _AA_dodgeCooldown;

    public float GetAAAttackCooldown => _AA_attackCooldown;
    
    public float GetAAPunchDamage => _AA_punchDamage;
    
    public float GetAAKickDamage => _AA_kickDamage;

    public float GetDADodgeCooldown => _DA_dodgeCooldown;

    public float GetDAAttackCooldown => _DA_attackCooldown;

    public float GetDABlockCooldown => _DA_blockCooldown;
    
    public float GetDAFleeCooldown => _DA_fleeCooldown;
    
    public float GetDAPunchDamage => _DA_punchDamage;

    #endregion
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
