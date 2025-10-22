using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseHealthUI : MonoBehaviour
{
    public Slider healthSlider;       
    public TMP_Text healthLabel;        

    void Start()
    {
        if (BaseHealth.Instance == null) { enabled = false; return; }
        var bh = BaseHealth.Instance;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = bh.maxHealth;
            healthSlider.value = bh.currentHealth;
        }
        if (healthLabel != null)
            healthLabel.text = $"{bh.currentHealth} / {bh.maxHealth}";

        bh.OnHealthChanged += OnBaseHealthChanged;
    }

    void OnDestroy()
    {
        if (BaseHealth.Instance != null)
            BaseHealth.Instance.OnHealthChanged -= OnBaseHealthChanged;
    }

    void OnBaseHealthChanged(int current, int max)
    {
        if (healthSlider != null) healthSlider.value = current;
        if (healthLabel != null) healthLabel.text = $"{current} / {max}";
    }
}
