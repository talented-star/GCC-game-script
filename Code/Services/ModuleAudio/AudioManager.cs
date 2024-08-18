using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Sources
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        private AudioMixerGroup soundGroup;
        private AudioMixerGroup musicGroup;
        private AudioMixerGroup voicecGroup;

        private Dictionary<string, AudioData> sounds = new Dictionary<string, AudioData>();
        private Dictionary<string, AudioData> music = new Dictionary<string, AudioData>();

        private List<AudioSource> soundSources = new();
        private List<Sound3DPoint> sound3DSources = new();

        private AudioSource voiceSource;
        private AudioSource musicSource1;
        private AudioSource musicSource2;

        private bool isPlayAllMusic = false;

        public bool IsSoundMute => PlayerPrefs.GetInt("SoundVolume", 1) != 1;
        public bool IsMusicMute => PlayerPrefs.GetInt("MusicVolume", 1) != 1;

        [Inject]
        private void Construct(AudioConfig audioConfig)
        {
            if (audioConfig == null)
            {
                Debug.LogError("Can't find audio list");
                return;
            }

            soundGroup = audioConfig.SoundMixerGroup;
            musicGroup = audioConfig.MusicMixerGroup;
            voicecGroup = audioConfig.VoiceMixerGroup;

            foreach (AudioData sound in audioConfig.Sounds)
                sounds[sound.Key] = sound;

            foreach (AudioData music in audioConfig.Music)
                this.music[music.Key] = music;

            soundSources.Add(gameObject.AddComponent<AudioSource>());
            soundSources[0].outputAudioMixerGroup = soundGroup;

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.outputAudioMixerGroup = voicecGroup;
            voiceSource.loop = false;

            musicSource1 = gameObject.AddComponent<AudioSource>();
            musicSource1.outputAudioMixerGroup = musicGroup;
            musicSource1.loop = true;

            musicSource2 = gameObject.AddComponent<AudioSource>();
            musicSource2.outputAudioMixerGroup = musicGroup;
            musicSource2.loop = false;

            Instance = this;
        }

        private void Start()
        {
            SetSound2DIsOn(PlayerPrefs.GetInt("SoundVolume", 1) == 1);
            SetMusic2DIsOn(PlayerPrefs.GetInt("MusicVolume", 1) == 1);
            SetVoice2DIsOn(PlayerPrefs.GetInt("VoiceVolume", 1) == 1);

            SetSoundVolume(PlayerPrefs.GetFloat("SoundVolumeValue", 0));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolumeValue", 0));
        }

        #region Sound3D
        public void PlaySound3D(string soundName, Vector3 worldPosition, bool isOnlyScene = true, bool isLoop = false)
        {
            if (!sounds.ContainsKey(soundName))
            {
                Debug.LogError("There is no sound with name " + soundName);
                return;
            }

            var point = Get3DAudioSource();
            point.source.outputAudioMixerGroup = soundGroup;

            point.PlaySound3D(sounds[soundName], worldPosition, isOnlyScene, isLoop);
        }

        private Sound3DPoint Get3DAudioSource()
        {
            if (sound3DSources.Count > 0)
                foreach (var point in sound3DSources)
                    if (!point.source.isPlaying)
                    {
                        var returned = point;
                        return returned;
                    }
            var newPoint = new Sound3DPoint(transform);
            sound3DSources.Add(newPoint);
            //audioSource.outputAudioMixerGroup = soundGroup;
            return newPoint;
        }
        #endregion Sound3D

        #region Sound
        public void PlayVoice(AudioClip clip, float volume = 1f)
        {
            voiceSource.PlayOneShot(clip, volume);
        }

        public void PlaySound(AudioClip clip)
        {
            PlaySound(clip.name);
        }

        public void PlaySound(string soundName)
        {
            if (!sounds.ContainsKey(soundName))
            {
                Debug.LogError("There is no sound with name " + soundName);
                return;
            }

            AudioData audio = sounds[soundName];
            var source = GetAudioSource();
            source.PlayOneShot(audio.Clip, audio.Volume);
        }

        private AudioSource GetAudioSource()
        {
            if (soundSources.Count > 0)
                foreach (var sound in soundSources)
                    if (!sound.isPlaying)
                    {
                        var returned = sound;
                        return returned;
                    }

            var audioSource = gameObject.AddComponent<AudioSource>();
            soundSources.Add(audioSource);
            audioSource.outputAudioMixerGroup = soundGroup;
            return audioSource;
        }
        #endregion Sound

        #region Music
        public void PlayMusic1(string musicName)
        {
            musicSource1.clip = music[musicName].Clip;
            musicSource1.volume = music[musicName].Volume;
            musicSource1.Play();
        }

        public void PlayMusic2(string musicName)
        {
            musicSource2.loop = true;
            StopAllMusic();
            musicSource2.clip = music[musicName].Clip;
            musicSource2.volume = music[musicName].Volume;
            musicSource2.Play();
        }

        public void PlayAllMusic()
        {
            isPlayAllMusic = true;
            StartCoroutine(PlayAllMusicRoutine());
        }

        private void StopAllMusic()
        {
            isPlayAllMusic = false;
        }

        private IEnumerator PlayAllMusicRoutine()
        {
            if (music == null || music.Count == 0)
            {
                Debug.LogError("There is no music");
                yield break;
            }

            musicSource2.loop = false;
            while (isPlayAllMusic)
            {
                foreach (var audioData in music.Values)
                {
                    musicSource2.clip = audioData.Clip;
                    musicSource2.volume = audioData.Volume;
                    musicSource2.Play();

                    while (musicSource2.isPlaying)
                        yield return null;
                }
            }
        }
        #endregion Music

        #region Volume
        public void SetSound2DIsOn(bool isOn)
        {
            Debug.Log("Sound: " + isOn);
            PlayerPrefs.SetInt("SoundVolume", isOn ? 1 : 0);
            soundGroup.audioMixer.SetFloat("SoundVolume", isOn ? 0f : -80f);
        }

        public void SetMusic2DIsOn(bool isOn)
        {
            Debug.Log("Music: " + isOn);
            PlayerPrefs.SetInt("MusicVolume", isOn ? 1 : 0);
            musicGroup.audioMixer.SetFloat("MusicVolume", isOn ? 0f : -80f);
        }

        public void SetVoice2DIsOn(bool isOn)
        {
            Debug.Log("Voice: " + isOn);
            PlayerPrefs.SetInt("VoiceVolume", isOn ? 1 : 0);
            musicGroup.audioMixer.SetFloat("VoiceVolume", isOn ? 0f : -80f);
        }

        public void SetSoundVolume(float volume)
        {
            //Debug.Log("Sound: " + isOn);
            PlayerPrefs.SetFloat("SoundVolumeValue", volume);
            soundGroup.audioMixer.SetFloat("SoundVolume", volume);
        }

        public void SetMusicVolume(float volume)
        {
            //Debug.Log("Music: " + isOn);
            PlayerPrefs.SetFloat("MusicVolumeValue", volume);
            musicGroup.audioMixer.SetFloat("MusicVolume", volume);
        }

        public void SetVoiceVolume(float volume)
        {
            //Debug.Log("Music: " + isOn);
            musicGroup.audioMixer.SetFloat("VoiceVolume", volume);
        }
        #endregion Volume
    }
}

