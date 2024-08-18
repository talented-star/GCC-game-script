using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LocalizationSprite : LocObject
{
    [SerializeField] private string _key;

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
        _key = newKey;
        OnEnable();
    }

    public override void OnSetLanguage()
    {
        SetLanguage(this.GetComponent<Image>(), settingsConfig.GetSprite(_key));
    }

    public static Image SetLanguage(Image to, Sprite key)
    {
        //Sprite sprite = Resources.Load<Sprite>(key);
        to.sprite = key;
        return to;
    }
}
