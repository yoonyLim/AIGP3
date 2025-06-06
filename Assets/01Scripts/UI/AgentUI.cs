using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentUI : MonoBehaviour
{
    [SerializeField] protected AgentType agentType;
    
    [SerializeField] Slider healthbar;
    [SerializeField] Slider easeHealthbar;
    [SerializeField] TextMeshProUGUI healthbarText;
    [SerializeField] Slider attackCooldown;
    [SerializeField] TextMeshProUGUI attackCooldownText;
    [SerializeField] Slider dodgeCooldown;
    [SerializeField] TextMeshProUGUI dodgeCooldownText;
    [SerializeField] [CanBeNull] Slider blockCooldown;
    [SerializeField] [CanBeNull] TextMeshProUGUI blockCooldownText;

	[SerializeField] GameObject GameOver;
    
    [Header("UI Effects")]
    [SerializeField] private float lerpSpeed = 0.1f;
    
    public void UpdateHealthbar(float health)
    {
        healthbar.value = health;
    }

    public void UpdateHealthbarText(float health)
    {
        healthbarText.text = Mathf.Ceil( Mathf.Clamp(health, 0f, 100f)).ToString("0");
    }
    
    public void UpdateDodgeCooldown(float cooldown)
    {
        if (agentType == AgentType.Attack)
            dodgeCooldown.value =  GameManager.Instance.GetAADodgeCooldown - cooldown;
        else if (agentType == AgentType.Defense)
            dodgeCooldown.value = GameManager.Instance.GetDADodgeCooldown - cooldown;
    }
    
    public void UpdateDodgeCooldownText(float cooldown)
    {
        if (agentType == AgentType.Attack)
            dodgeCooldownText.text =  Mathf.Clamp(GameManager.Instance.GetAADodgeCooldown - cooldown, 0f, GameManager.Instance.GetAADodgeCooldown).ToString("F1");
        else if (agentType == AgentType.Defense)
            dodgeCooldownText.text =  Mathf.Clamp(GameManager.Instance.GetDADodgeCooldown - cooldown, 0f, GameManager.Instance.GetDADodgeCooldown).ToString("F1");
    }
    
    public void UpdateAttackCooldownText(float cooldown)
    {
        if (agentType == AgentType.Attack)
            attackCooldownText.text =  Mathf.Clamp(GameManager.Instance.GetAAAttackCooldown - cooldown, 0f, GameManager.Instance.GetAAAttackCooldown).ToString("F1");
        else if (agentType == AgentType.Defense)
            attackCooldownText.text =  Mathf.Clamp(GameManager.Instance.GetDAAttackCooldown - cooldown, 0f, GameManager.Instance.GetDAAttackCooldown).ToString("F1");
    }
    
    public void UpdateBlockCooldownText(float cooldown)
    {
        if (blockCooldown && blockCooldownText)
            blockCooldownText.text =  Mathf.Clamp(GameManager.Instance.GetDABlockCooldown - cooldown, 0f, GameManager.Instance.GetDABlockCooldown).ToString("F1");
    }

    public void UpdateAttackCooldown(float cooldown)
    {
        if (agentType == AgentType.Attack)
            attackCooldown.value = GameManager.Instance.GetAAAttackCooldown - cooldown;
        else if (agentType == AgentType.Defense)
            attackCooldown.value = GameManager.Instance.GetDAAttackCooldown - cooldown;
    }

    public void UpdateBlockCooldown(float cooldown)
    {
        if (blockCooldown) 
            blockCooldown.value = GameManager.Instance.GetDABlockCooldown - cooldown;
    }

    private void Start()
    {
        if (agentType == AgentType.Attack)
        {
            dodgeCooldown.maxValue = GameManager.Instance.GetAADodgeCooldown;
            attackCooldown.maxValue = GameManager.Instance.GetAAAttackCooldown;
        } 
        else if (agentType == AgentType.Defense)
        {
            dodgeCooldown.maxValue = GameManager.Instance.GetDADodgeCooldown;
            attackCooldown.maxValue = GameManager.Instance.GetDAAttackCooldown;
            if (blockCooldown) blockCooldown.maxValue = GameManager.Instance.GetDABlockCooldown;
        }
        
        GameOver.SetActive(false);
    }

    private void Update()
    {
        if (!Mathf.Approximately(easeHealthbar.value, healthbar.value))
            easeHealthbar.value = Mathf.Lerp(easeHealthbar.value, healthbar.value, lerpSpeed);

        dodgeCooldownText.enabled = !Mathf.Approximately(dodgeCooldown.value, 0f);
        attackCooldownText.enabled = !Mathf.Approximately(attackCooldown.value, 0f);
        if (blockCooldown && blockCooldownText)
            blockCooldownText.enabled = !Mathf.Approximately(blockCooldown.value, 0f);

        if (healthbar.value <= 0f)
            GameOver.SetActive(true);
    }
}
