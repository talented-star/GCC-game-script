using System;
using System.Collections.Generic;

namespace Config
{
    [Serializable]
    public struct LocTextValue
    {
        public string key;
        public List<TextPair> texts;
    }

    [Serializable]
    public struct LocSpriteValue
    {
        public string key;
        public List<SpritePair> sprites;
    }

    [Serializable]
    public struct LocAudioValue
    {
        public string key;
        public List<AudioPair> audios;
    }
}