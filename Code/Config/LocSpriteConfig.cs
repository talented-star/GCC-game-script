using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "LocSpriteConfig", menuName = "ScriptableObjects/LocSpriteConfig", order = 1)]
    public class LocSpriteConfig : ScriptableObject
    {
        [SerializeField] private SystemLanguage currentLanguage = SystemLanguage.English;
        [SerializeField] private List<LocSpriteValue> sprites;

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

        public Sprite GetSprite(string key) => string.IsNullOrEmpty(key) ? default :
            sprites.FirstOrDefault(step => step.key.Equals(key))
            .sprites.FirstOrDefault(pair => pair.langName == currentLanguage).sprite;
    }
}