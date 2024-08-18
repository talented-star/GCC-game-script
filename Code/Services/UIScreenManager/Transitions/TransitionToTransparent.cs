using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.ScreenManager.Transitions
{
    public class TransitionToTransparent : ITransition
    {
        private const float TRANSITION_TIME = 0.05f;
        private const float ALPHA_MAX = 1f;
        private const float ALPHA_MIN = 0f;
        private const string BLOCK_CLICK_PANEL_NAME = "TransitionOverlay";
        private GameObject _gameObject;
        private CanvasGroup _canvasGroup;
        private GameObject _blockClicksPanel;

        public TransitionToTransparent(GameObject gameObject, Image stretchedImagePrefab)
        {
            _gameObject = gameObject;
            if (_gameObject.GetComponent<CanvasGroup>() != null)
                _canvasGroup = _gameObject.GetComponent<CanvasGroup>();
            else
                _canvasGroup = _gameObject.AddComponent<CanvasGroup>();
            CreateBlockClickPanel(stretchedImagePrefab);
        }

        public async UniTask Show(ITransition prevOrNull)
        {
            _gameObject.SetActive(false);
            
            if (prevOrNull != null)
                await prevOrNull.Hide();
            
            await Fade(ALPHA_MIN, ALPHA_MAX, TRANSITION_TIME);
        }

        public async UniTask Hide()
        {
            await Fade(ALPHA_MAX, ALPHA_MIN, TRANSITION_TIME);
            _gameObject.SetActive(false);
        }

        private async UniTask Fade(float from, float to, float durationSec)
        {
            DisableInteractable();
            _gameObject.SetActive(true);
            var progress = 0f;
            while(progress < 1f)
            {
                progress += Time.deltaTime / durationSec;
                _canvasGroup.alpha = Mathf.Lerp(from, to, progress);
                await UniTask.DelayFrame(1);
            }
            EnableInteractable();
        }

        private void CreateBlockClickPanel(Image prefab)
        {
            var image = GameObject.Instantiate(prefab, _gameObject.transform);
            image.sprite = null;
            image.color = new Color(0, 0, 0, 0);
            _blockClicksPanel = image.gameObject;
            _blockClicksPanel.name = BLOCK_CLICK_PANEL_NAME;
            EnableInteractable();
        }

        private void DisableInteractable()
        {
            _blockClicksPanel.transform.SetAsLastSibling();
            _blockClicksPanel.SetActive(true);
        }

        private void EnableInteractable()
        {
            _blockClicksPanel.SetActive(false);
        }
    }
}