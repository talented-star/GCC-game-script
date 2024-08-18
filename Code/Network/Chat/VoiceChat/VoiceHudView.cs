using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.Services.Chat.VoiceChat
{
    public class VoiceHudView : MonoBehaviour
    {
        [SerializeField]
        private Image Active;
        [SerializeField]
        private Image Default;

        private const float thresholdEndPlaying = 0.15f;
        private float _delayed;


        public void OnPlaying()
        {
            Active.gameObject.SetActive(true);
            Default.gameObject.SetActive(false);
            ResetDelaying();
        }

        #region private
        private void Start()
        {
            OnEndPlaying();
        }

        private void OnEndPlaying()
        {
            Active.gameObject?.SetActive(false);
            Default.gameObject.SetActive(true);
        }

        private void ResetDelaying()
        {
            _delayed = 0;
        }

        private void Update()
        {
            if (thresholdEndPlaying < _delayed)
            {
                if (Active.IsActive())
                {
                    OnEndPlaying();
                }
                ResetDelaying();
            }
            _delayed += Time.deltaTime;
        }

        #endregion
    }
}