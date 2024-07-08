using TMPro;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    private RectTransform _level;
    private TextMeshProUGUI _levelText;

    void Start()
    {
        _level = transform.GetChild(0).transform.GetChild(1).GetComponent<RectTransform>();
        _levelText = transform.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    public void SetExp(short level, float exp, float maxExp)
    {
        if (exp > maxExp) return;
        float per = exp / maxExp;
        _level.localScale = new Vector3(per, 1, 1);
        SetLevelText(level, exp, maxExp);
    }

    private void SetLevelText(short level, float exp, float maxExp)
    {
        _levelText.text = $"Lv.{level} ({exp / maxExp * 100:0.00}%)";
    }
}