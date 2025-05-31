using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Slider HealthSlider;

    public float MaxHealth = 100f;
    public float CurrentHealth;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Mathf.Approximately(HealthSlider.value, CurrentHealth))
        {
            HealthSlider.value = CurrentHealth;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            TakeDamage(10f);
    }

    void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
    }
}
