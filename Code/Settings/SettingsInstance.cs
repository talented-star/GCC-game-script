using ModestTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SettingsInstance", menuName = "ScriptableObjects/SettingsInstance", order = 1)]
public class SettingsInstance : ScriptableObject
{
    public float mouseSensitivity = 3f;
    
    public float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat("mouseSensitivity", 3f);
    }
    
    public float GetScreenMode()
    {
        return PlayerPrefs.GetInt("screenMode", (int)Screen.fullScreenMode);
    }
    
    public float GetScreenResolution()
    {
        return PlayerPrefs.GetInt("screenResolution", Screen.resolutions.IndexOf(Screen.currentResolution));
    }
    
    public float GetGraphicsQuality()
    {
        return PlayerPrefs.GetInt("graphicsQuality", QualitySettings.GetQualityLevel());
    }
}
