using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform _health;

    void Awake() {
        _health = transform.GetChild(1);
    }

    public void SetHealth(short health, short maxHealth) {
        _health.localScale = new Vector3(health / maxHealth, 1, 1);
    }
}
