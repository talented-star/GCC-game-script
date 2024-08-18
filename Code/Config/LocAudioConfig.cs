using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "LocAudioConfig", menuName = "ScriptableObjects/LocAudioConfig", order = 1)]
    public class LocAudioConfig : ScriptableObject
    {
        [SerializeField] private SystemLanguage currentLanguage;
        [SerializeField] private List<LocAudioValue> audios;

        public SystemLanguage CurrentLanguage => currentLanguage;

        public void SetLanguage(SystemLanguage language) => currentLanguage = language;

        public void CheckLanguage()
        {
            currentLanguage = Application.systemLanguage switch
            {
                SystemLanguage.Russian => SystemLanguage.Russian,
                _ => SystemLanguage.English
            };
        }

        public AudioClip GetAudio(string nameStep) => //string.IsNullOrEmpty(nameStep) ? "" :
            audios.FirstOrDefault(step => step.key.Equals(nameStep))
            .audios.FirstOrDefault(pair => pair.langName == currentLanguage).audio;
    }
}