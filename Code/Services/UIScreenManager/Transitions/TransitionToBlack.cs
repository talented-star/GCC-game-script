using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.ScreenManager.Transitions
{
    public class TransitionToBlack: ITransition
    {
        private const float MAX_ALPHA = 1f;
        private const float MIN_ALPHA = 0F;
        private const float TRANSITION_DURATION = 1F;
        private const string COLOR_PANEL_NAME = "TransitionOverlay";
        private GameObject _gameObject;
        private CanvasGroup _canvasGroup;
        private CanvasGroup _colorPanelCanvasGroup;
        private Image _colorPanel;

        public TransitionToBlack(GameObject gameObject, Image colorPanelPrefab)
        {
            _gameObject = gameObject;
            if (_gameObject.GetComponent<CanvasGroup>() != null)
                throw new Exception("GameObject already contains CanvasGroup. TransitionToBlack cant initialize.");
            _canvasGroup = _gameObject.AddComponent<CanvasGroup>();
            CreateColorOverlay(colorPanelPrefab);
        }

        public async UniTask Show(ITransition prevOrNull)
        {
            if (prevOrNull == null)
            {
                _canvasGroup.alpha = MAX_ALPHA;
                await Fade(_colorPanelCanvasGroup, MAX_ALPHA, MIN_ALPHA, TRANSITION_DURATION);
            }
            else
            {
                _gameObject.SetActive(false);
                await prevOrNull.Hide();
                await Fade(_colorPanelCanvasGroup, MAX_ALPHA, MIN_ALPHA, TRANSITION_DURATION);
            }
            EnableInteractable();
        }

        public async UniTask Hide()
        {
            await Fade(_colorPanelCanvasGroup, MIN_ALPHA, MAX_ALPHA, TRANSITION_DURATION);
            _gameObject.SetActive(false);
        }

        private async UniTask Fade(CanvasGroup target, float from, float to, float durationSec)
        {
            DisableInteractable();
            _gameObject.SetActive(true);
            var progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime / durationSec;
                target.alpha = Mathf.Lerp(from, to, progress);
                await UniTask.DelayFrame(1);
            }
        }

        private void CreateColorOverlay(Image stretchedImagePrefab)
        {
            _colorPanel = GameObject.Instantiate(stretchedImagePrefab, _gameObject.transform);
            _colorPanel.color = Color.black;
            _colorPanel.name = COLOR_PANEL_NAME;
            _colorPanelCanvasGroup = _colorPanel.gameObject.AddComponent<CanvasGroup>();
            _colorPanelCanvasGroup.ignoreParentGroups = true;
            EnableInteractable();
        }

        private void DisableInteractable()
        {
            _colorPanel.gameObject.transform.SetAsLastSibling();
            _colorPanel.gameObject.SetActive(true);
        }

        private void EnableInteractable()
        {
            _colorPanel.gameObject.SetActive(false);
        }
    }
}