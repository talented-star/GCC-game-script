using UnityEngine;

namespace GrabCoin.Services.Chat.VoiceChat
{
    public class VoiceListener: MonoBehaviour
    {
        [SerializeField] private AudioSource audioSourceEar;
 
        public void OnRecived(float[] audioData)
        {
            AudioClip clip = AudioClip.Create("ear", audioData.Length, 1, (int)VoiceRecorder._samplingRate, false);
            clip.SetData(audioData, 0);
            audioSourceEar.clip = clip;
            if (!audioSourceEar.isPlaying)
            {
                audioSourceEar.Play();
            }
        }
    }
}
