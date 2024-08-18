using NaughtyAttributes;
using System;
using UnityEngine;

namespace Config
{
    [Serializable]
    public struct TextPair
    {
        public SystemLanguage langName;
        [TextArea(3, 10)]public string text;
    }
    [Serializable]
    public struct SpritePair
    {
        public SystemLanguage langName;
        [ShowAssetPreview] public Sprite sprite;
    }

    [Serializable]
    public struct AudioPair
    {
        public SystemLanguage langName;
        public AudioClip audio;
    }
}