using UnityEngine;
using UnityEngine.UI;
using static ColorUtils;

public class HealthBar : MonoBehaviour
{
    private RectTransform _health;

    void Awake()
    {
        _health = transform.GetChild(0).transform.GetChild(1).GetComponent<RectTransform>();
    }

    public void SetHealth(short health, short maxHealth)
    {
        if (health > maxHealth) return;
        _health.localScale = new Vector3((float)health / maxHealth, 1, 1);
        float leftHealth = (float)health / maxHealth;
        var image = _health.GetComponent<Image>();
        if (leftHealth <= 0.2f) image.color = HexToColor("950000");
        else if (leftHealth <= 0.4f) image.color = HexToColor("C15200");
        else if (leftHealth <= 0.6f) image.color = HexToColor("ECD400");
        else if (leftHealth <= 0.8f) image.color = HexToColor("7E9300");
    }
}
