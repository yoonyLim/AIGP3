using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentUI : MonoBehaviour
{
    [SerializeField] Slider healthbar;
    [SerializeField] Slider easeHealthbar;
    [SerializeField] TextMeshProUGUI healthbarText;
    [SerializeField] Slider attackCooldown;
    [SerializeField] Slider dodgeCooldown;
    [SerializeField] [CanBeNull] Slider blockCooldown;
    
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

    public void UpdateAttackCooldown(float cooldown)
    {
        attackCooldown.value = 2.5f - cooldown;
    }
    
    public void UpdateDodgeCooldown(float cooldown)
    {
        dodgeCooldown.value = 5f - cooldown;
    }

    public void UpdateBlockCooldown(float cooldown)
    {
        if (blockCooldown != null) blockCooldown.value = 2.5f - cooldown;
    }

    private void Update()
    {
        if (!Mathf.Approximately(easeHealthbar.value, healthbar.value))
            easeHealthbar.value = Mathf.Lerp(easeHealthbar.value, healthbar.value, lerpSpeed);
    }
}
