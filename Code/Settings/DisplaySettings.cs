using ModestTree;
using Org.BouncyCastle.Tsp;
using Sources;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySettings : MonoBehaviour
{
    [SerializeField] private Slider _soundVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;

    private void Awake()
    {
        FillMouseSensitivity();
    }
    private void FillMouseSensitivity()
    {
        _soundVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolumeValue", 0);
        _soundVolumeSlider.onValueChanged.AddListener(delegate { SetSoundVolume(); });
        _musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolumeValue", 0);
        _musicVolumeSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });

        SetSoundVolume();
        SetMusicVolume();
    }

    private void SetSoundVolume()
    {
        AudioManager.Instance.SetSoundVolume(_soundVolumeSlider.value);
    }

    private void SetMusicVolume()
    {
        AudioManager.Instance.SetMusicVolume(_musicVolumeSlider.value);
    }
}
