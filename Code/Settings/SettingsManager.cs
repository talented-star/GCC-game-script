using GrabCoin.UI.ScreenManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _profileButton;
    [SerializeField] private Button _graphicsButton;
    [SerializeField] private Button _displayButton;
    [SerializeField] private Button _inputButton;

    [Header("Panels")]
    [SerializeField] private Image _profilePanel;
    [SerializeField] private Image _graphicsPanel;
    [SerializeField] private Image _displayPanel;
    [SerializeField] private Image _inputPanel;

    private void Start()
    {
        _profileButton.onClick.AddListener(ActivateProfileButton);
        _graphicsButton.onClick.AddListener(ActivateGraphicsPanel);
        _displayButton.onClick.AddListener(ActivateDisplayPanel);
        _inputButton.onClick.AddListener(ActivateInputPanel);
    }

    private void HideAllPanels()
    {
        _profilePanel.gameObject.SetActive(false);
        _graphicsPanel.gameObject.SetActive(false);
        _displayPanel.gameObject.SetActive(false);
        _inputPanel.gameObject.SetActive(false);
    }

    private void ActivateProfileButton()
    {
        HideAllPanels();
        _profilePanel.gameObject.SetActive(true);
    }

    private void ActivateGraphicsPanel()
    {
        HideAllPanels();
        _graphicsPanel.gameObject.SetActive(true);
    }

    private void ActivateDisplayPanel()
    {
        HideAllPanels();
        _displayPanel.gameObject.SetActive(true);
    }

    private void ActivateInputPanel()
    {
        HideAllPanels();
        _inputPanel.gameObject.SetActive(true);
    }
}
