using TMPro;
using UnityEngine;
using Zenject;

public class LocalizationUITMPText : LocObject
{
    [SerializeField] private string key;

    private ZenjectDynamicObjectInjection _zenjectDynamic;

    private void OnEnable()
    {
        if (_zenjectDynamic == null)
            _zenjectDynamic = gameObject.AddComponent<ZenjectDynamicObjectInjection>();
        settingsConfig.onShiftLanguage += OnSetLanguage;
        OnSetLanguage();
    }

    private void OnDisable()
    {
        settingsConfig.onShiftLanguage -= OnSetLanguage;
    }

    public override void SetNewKey(string newKey)
    {
        key = newKey;
        OnEnable();
    }

    public override void SetNewText(string newKey)
    {
        base.SetNewText(newKey);
        GetComponent<TMP_Text>().text = newKey;
    }

    public override void OnSetLanguage()
    {
        SetLanguage(GetComponent<TMP_Text>(), settingsConfig.GetUIText(key));
    }

    public TMP_Text SetLanguage(TMP_Text to, string key)
    {
        to.text = key;
        return to;
    }
}
