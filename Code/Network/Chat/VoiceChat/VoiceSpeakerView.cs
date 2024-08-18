using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.Services.Chat.VoiceChat
{
    public class VoiceSpeakerView : MonoBehaviour
    {
        [SerializeField]
        private Image icon;

        private const float thresholdEndPlaying = 0.15f;
        private float _delayed;
        public void OnPlaying()
        {
            icon.gameObject.SetActive(true);
            ResetDelaying();
        }

        private void OnEndPlaying()
        {
            icon.gameObject?.SetActive(false);
        }

        private void ResetDelaying()
        {
            _delayed = 0;
        }

        private void Update()
        {
            if(thresholdEndPlaying < _delayed)
            {
                OnEndPlaying();
                ResetDelaying();
            }
            _delayed += Time.deltaTime;
        }
    }
}