using UnityEngine;

public class FuelBar : MonoBehaviour
{
    private RectTransform _fuel;

    void Awake()
    {
        _fuel = transform.GetChild(0).transform.GetChild(1).GetComponent<RectTransform>();
    }

    public void SetFuel(float fuel, float maxFuel)
    {
        if (fuel > maxFuel) return;
        _fuel.localScale = new Vector3(fuel / maxFuel, 1, 1);
    }
}
