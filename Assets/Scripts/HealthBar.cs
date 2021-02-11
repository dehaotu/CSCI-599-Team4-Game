using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;
    public Gradient gradient;
    public Image fill;
    private void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        gradient.Evaluate(health);
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        fill.color = gradient.Evaluate(1f);
    }
}
