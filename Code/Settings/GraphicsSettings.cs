using ModestTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;
using System.Linq;

public class GraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _graphicsQuality;
    [SerializeField] private Slider _postprocessingPower;
    [SerializeField] private TMP_Dropdown _displayMode;
    [SerializeField] private TMP_Dropdown _displayResolution;

    private string _lastSceneName = "";
    private List<PostProcessVolume> _postProcesses = new();

    private void Awake()
    {
        FillGraphicsQualtity();
        FillPostprocessingPower();
        FillDisplayMode();
        FillDispalyResolution();
    }

    private void OnEnable()
    {
        if (!SceneManager.GetActiveScene().name.Equals(_lastSceneName))
            _postProcesses = FindObjectsOfType<PostProcessVolume>().ToList();
        _lastSceneName = SceneManager.GetActiveScene().name;
    }

    private void FillGraphicsQualtity()
    {
        _graphicsQuality.options.Clear();

        foreach (var name in QualitySettings.names)
        {
            _graphicsQuality.options.Add(new TMP_Dropdown.OptionData { text = name });
        }
        _graphicsQuality.value = PlayerPrefs.GetInt("graphicsQuality", QualitySettings.GetQualityLevel());

        _graphicsQuality.onValueChanged.AddListener(delegate { SetGraphicsQualtity(); });
        SetGraphicsQualtity();
    }

    private void FillPostprocessingPower()
    {
        _postProcesses = FindObjectsOfType<PostProcessVolume>().ToList();

        _postprocessingPower.value = PlayerPrefs.GetFloat("PostprocessingValue", 0.75f);
        _postprocessingPower.onValueChanged.AddListener(PostprocessingValue);

        foreach (var pp in _postProcesses)
            pp.weight = _postprocessingPower.value;
    }

    private void SetGraphicsQualtity()
    {
        QualitySettings.SetQualityLevel(_graphicsQuality.value);
        PlayerPrefs.SetInt("graphicsQuality", _graphicsQuality.value);
    }

    private void FillDisplayMode()
    {
        _displayMode.options.Clear();

        foreach (var name in System.Enum.GetNames(typeof(FullScreenMode)))
        {
            _displayMode.options.Add(new TMP_Dropdown.OptionData { text = name });
        }
        _displayMode.value = PlayerPrefs.GetInt("displayMode", (int)Screen.fullScreenMode);

        _displayMode.onValueChanged.AddListener(delegate { SetDisplay(); });
    }

    private void FillDispalyResolution()
    {
        _displayResolution.options.Clear();

        foreach (var res in Screen.resolutions)
        {
            var resolution = res;
            _displayResolution.options.Add(new TMP_Dropdown.OptionData { text = (resolution.width + "x" + resolution.height + " " + resolution.refreshRate + "Hz") });
        }
        _displayResolution.value = PlayerPrefs.GetInt("displayResolution", Screen.resolutions.IndexOf(Screen.currentResolution));

        _displayResolution.onValueChanged.AddListener(delegate { SetDisplay(); });
    }

    public void SetDisplay()
    {
        Screen.SetResolution(Screen.resolutions[_displayResolution.value].width,
            Screen.resolutions[_displayResolution.value].height,
            (FullScreenMode)_displayMode.value, Screen.resolutions[_displayResolution.value].refreshRate);
        PlayerPrefs.SetInt("displayMode", _displayMode.value);
        PlayerPrefs.SetInt("displayResolution", _displayResolution.value);
    }

    public void PostprocessingValue(float value)
    {
        PlayerPrefs.SetFloat("PostprocessingValue", value);

        foreach (var pp in _postProcesses)
            pp.weight = value;
    }
}
