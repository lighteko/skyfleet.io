using TMPro;
using UnityEngine;

public class AmmoBar : MonoBehaviour
{
    private RectTransform _ammo;
    private TextMeshProUGUI _ammoText;

    void Awake()
    {
        _ammo = transform.GetChild(0).transform.GetChild(1).GetComponent<RectTransform>();
        _ammoText = transform.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    public void SetAmmo(short ammo, short maxAmmo)
    {
        if (ammo > maxAmmo) return;
        float per = (float) ammo / maxAmmo;
        _ammo.localScale = new Vector3(per, 1, 1);
        SetAmmoText(ammo, maxAmmo);
    }

    private void SetAmmoText(short ammo, short maxAmmo)
    {
        _ammoText.text = $"{ammo} / {maxAmmo}";
    }
}
