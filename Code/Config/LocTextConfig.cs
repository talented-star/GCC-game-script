using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "LocTextConfig", menuName = "ScriptableObjects/LocTextConfig", order = 1)]
    public class LocTextConfig : ScriptableObject
    {
        [SerializeField] private SystemLanguage currentLanguage = SystemLanguage.English;
        [SerializeField] private List<LocTextValue> texts;

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

        public string GetText(string key) => string.IsNullOrEmpty(key) ? "" :
            texts.FirstOrDefault(step => step.key.Equals(key))
            .texts.FirstOrDefault(pair => pair.langName == currentLanguage).text;
    }
}