using UnityEngine;

public class FuelBar : MonoBehaviour
{
    private RectTransform _fuel;

    void Awake()
    {
        _fuel = transform.GetChild(0).transform.GetChild(1).GetComponent<RectTransform>();
    }

    public void SetFuel(short fuel, short maxFuel)
    {
        if (fuel > maxFuel) return;
        _fuel.localScale = new Vector3((float)fuel / maxFuel, 1, 1);
    }
}
