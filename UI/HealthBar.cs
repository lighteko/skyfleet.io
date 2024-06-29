using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private RectTransform _health;

    void Awake() {
        _health = transform.GetChild(1).GetComponent<RectTransform>();
    }

    public void SetHealth(short health, short maxHealth) {
        _health.localScale = new Vector3((float) health / maxHealth, 1, 1);
    }
}
