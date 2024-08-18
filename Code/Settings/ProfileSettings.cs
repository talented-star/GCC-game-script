using PlayFab;
using PlayFab.ClientModels;
using Sources;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class ProfileSettings : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nickName;
    [SerializeField] private TMP_Dropdown _language;

    [Inject] private SettingsConfig _settingsConfig;

    private void Awake()
    {
        _language.onValueChanged.AddListener(ChangeLanguage);
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result =>
            {
                string name = result.AccountInfo.TitleInfo.DisplayName;
                if (string.IsNullOrWhiteSpace(name))
                    name = result.AccountInfo.Username;
                FillNickName(name);
            }, Debug.LogError);
    }

    private void FillNickName(string name)
    {
        _nickName.text = name;
        _nickName.onEndEdit.AddListener(SetNickName);
    }
    
    private void SetNickName(string name)
    {
        Debug.Log("Nick Name Implemented " + _nickName.text);
        if (string.IsNullOrWhiteSpace(name)) return;
        PlayFabClientAPI.UpdateUserTitleDisplayName(new PlayFab.ClientModels.UpdateUserTitleDisplayNameRequest
        {
            DisplayName = _nickName.text
        }, result =>
        {
            Translator.Send(UIPlayerProtocol.ChangeName, new StringData { value = result.DisplayName});
        }, Debug.LogError);
    }

    private void ChangeLanguage(int value)
    {
        _settingsConfig.CurrentLanguage = _settingsConfig.Languages[value];
    }
}
