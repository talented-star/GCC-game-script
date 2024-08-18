using Sources;
using UnityEngine;
using Zenject;

public abstract class LocObject : MonoBehaviour
{
    [Inject] protected SettingsConfig settingsConfig;

    public virtual void SetNewKey(string newKey) { }

    public virtual void SetNewText(string newKey) { }

    public abstract void OnSetLanguage();
}
