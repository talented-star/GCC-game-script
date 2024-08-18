using Config;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationDropbox : LocObject
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
        SetLanguage(GetComponent<TMP_Dropdown>(), settingsConfig.GetUIText(key));
    }

    public static TMP_Dropdown SetLanguage(TMP_Dropdown to, string key)
    {
        int dropdownValue = to.value;
        List<string> keys = new List<string>();
        keys.AddRange(key.Split(new char[] { ':' }));
        
        to.ClearOptions();
        to.AddOptions(keys);

        to.value = dropdownValue;

        return to;
    }
}
