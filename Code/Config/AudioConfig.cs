#pragma warning disable 649
using UnityEngine;
using UnityEngine.Audio;

namespace Sources
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "ScriptableObjects/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private AudioMixerGroup soundMixerGroup;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup voiceMixerGroup;
        [SerializeField] private AudioData[] sounds;
        [SerializeField] private AudioData[] music;
        [SerializeField] private AudioData[] voice;

        public AudioMixerGroup SoundMixerGroup => soundMixerGroup;
        public AudioMixerGroup MusicMixerGroup => musicMixerGroup;
        public AudioMixerGroup VoiceMixerGroup => voiceMixerGroup;
        public AudioData[] Sounds => sounds;
        public AudioData[] Music => music;
        public AudioData[] Voice => voice;
    }
}