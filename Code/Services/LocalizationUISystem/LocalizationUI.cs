using UnityEngine;
using UnityEngine.UI;

public class LocalizationUI : LocObject
{
    [SerializeField] private string key;

    private void OnEnable()
    {
        settingsConfig.onShiftLanguage += OnSetLanguage;
        OnSetLanguage();
    }

    private void OnDisable()
    {
        settingsConfig.onShiftLanguage -= OnSetLanguage;
    }

    public override void OnSetLanguage()
    {
        SetLanguage(GetComponent<Text>(), settingsConfig.GetUIText(key));
    }

    public Text SetLanguage(Text to, string key)
    {
        to.text = key;
        return to;
    }
}
