using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/PreloaderScreen.prefab")]
    public class PreloaderScreen : UIScreenBase
    {
        public event Action OnComplete;
        [SerializeField] private Image _scaleImage;

        private const float PROCESS_DURATION = 3f;

        public override void CheckOnEnable()
        {

        }

        public async UniTask<bool> Process()
        {
            await ProcessCoroutine();
            return true;
        }

        private IEnumerator ProcessCoroutine()
        {
            var progress = 0f;

            while (progress <= 1 && _scaleImage != null)
            {
                progress += Time.deltaTime / PROCESS_DURATION;
                if (_scaleImage != null)
                {
                    _scaleImage.fillAmount = progress;                
                }
                yield return null;
            }

            OnComplete?.Invoke();
        }
    }
}