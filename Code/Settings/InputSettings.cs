using GrabCoin.Services.Chat.VoiceChat;
using GrabCoin.UI.Screens;
using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InputSettings : MonoBehaviour
{
    [SerializeField] private Slider _mouseSensitivitySlider;
    [SerializeField] private TMP_Text _mouseSensitivityText;
    [SerializeField] private TMP_Dropdown _microphoneDevices;

    [Foldout("Key input settings")]
    [SerializeField] private TMP_Text _controlSchemeText;
    [Foldout("Key input settings")]
    [SerializeField] private Button _nextControlSchemeButton;
    [Foldout("Key input settings")]
    [SerializeField] private Button _prevControlSchemeButton;
    [Foldout("Key input settings")]
    [SerializeField] private KeyBindSlot _prefabKeyInputSlot;
    [Foldout("Key input settings")]
    [SerializeField] private Transform _keyBindSlotContainer;

    private KeyInputSettings _keySettings;
    private int _indexControlScheme;

    [Inject]
    private void Construct(
        KeyInputSettings keySettings
        )
    {
        _keySettings = keySettings;
    }

    private void Awake()
    {
        FillMouseSensitivity();
        FillMicrophoneDevices();
        FillKeySettings();
    }

    private void FillKeySettings()
    {
        _controlSchemeText.text = _keySettings.GetControlSchemes()[_indexControlScheme];
        _nextControlSchemeButton.onClick.AddListener(NextControlScheme);
        _prevControlSchemeButton.onClick.AddListener(PrevControlScheme);

        FillKeyBindings();
    }

    private void FillKeyBindings()
    {
        var schemesActionsData = _keySettings.GetSchemesActionsData(_controlSchemeText.text);
        foreach (var action in schemesActionsData)
        {
            foreach (var bind in action.Value)
            {
                var slot = Instantiate(_prefabKeyInputSlot, _keyBindSlotContainer);
                slot.Populate(bind);
            }
        }
    }

    private void CleanKeyBindings()
    {
        int count = _keyBindSlotContainer.childCount;
        for (int i = count - 1; i >= 0; i--)
            Destroy(_keyBindSlotContainer.GetChild(i).gameObject);
    }

    private void NextControlScheme()
    {
        _indexControlScheme = ++_indexControlScheme % _keySettings.GetControlSchemes().Length;
        _controlSchemeText.text = _keySettings.GetControlSchemes()[Mathf.Abs(_indexControlScheme)];
        CleanKeyBindings();
        FillKeyBindings();
    }

    private void PrevControlScheme()
    {
        _indexControlScheme = --_indexControlScheme % _keySettings.GetControlSchemes().Length;
        _controlSchemeText.text = _keySettings.GetControlSchemes()[Mathf.Abs(_indexControlScheme)];
        CleanKeyBindings();
        FillKeyBindings();
    }

    private void FillMouseSensitivity()
    {
        _mouseSensitivitySlider.value = PlayerPrefs.GetFloat("mouseSensitivity", 3);
        _mouseSensitivitySlider.onValueChanged.AddListener(delegate { SetMouseSensitivity(); });

        UpdateMouseSensitivityText();
    }

    private void SetMouseSensitivity()
    {
        PlayerPrefs.SetFloat("mouseSensitivity", _mouseSensitivitySlider.value);
        UpdateMouseSensitivityText();
    }

    private void UpdateMouseSensitivityText()
    {
        _mouseSensitivityText.text = string.Format("{0:f1}", _mouseSensitivitySlider.value);
    }

    private void FillMicrophoneDevices()
    {
        _microphoneDevices.options.Clear();

        _microphoneDevices.AddOptions(VoiceRecorder.micOptionsStrings);
        _microphoneDevices.onValueChanged.RemoveAllListeners();
        _microphoneDevices.value = PlayerPrefs.GetInt("microphoneDevices", 0);
        _microphoneDevices.onValueChanged.AddListener(delegate { MicDropdownValueChanged(VoiceRecorder.micOptions[_microphoneDevices.value]); });
    }

    private void MicDropdownValueChanged(MicRef mic)
    {
        VoiceRecorder.MicrophoneDevice = mic.Device;
        PlayerPrefs.SetInt("microphoneDevices", _microphoneDevices.value);
    }

    public void SetMicrophoneDevices()
    {
        PlayerPrefs.SetInt("microphoneDevices", _microphoneDevices.value);
    }
}
