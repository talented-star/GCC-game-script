using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.HUD
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WorldToScreenBar : MonoBehaviour
    {
        [SerializeField] private Image _filligBar;
        [SerializeField] private TMP_Text _nameText;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Transform _target;

        public bool activeSelf => _canvasGroup.alpha == 1;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            Hide(true);
        }

        private void Update()
        {
            if (_target != null)
            {
                _rectTransform.position = Camera.main.WorldToScreenPoint(_target.position + Vector3.up);
            }
        }

        public void FollowWorldPosition(Transform target, string name)
        {
            if (_target != target || _filligBar.fillAmount <= 0)
                Hide(true);
            _target = target;
            _nameText.text = name;
        }

        public void StopFollow()
        {
            _target = null;
            Hide();
        }

        public void SetBarValue(float value, string name)
        {
            _filligBar.fillAmount = value;
            if (!string.IsNullOrEmpty(name))
                _nameText.text = name;
            SetColor(value);
            if (value > 0)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            if (_canvasGroup.alpha == 0)
                Fade(0, 1, 0.2f).Forget();
        }

        private void Hide(bool isForce = false)
        {
            if (isForce)
                _canvasGroup.alpha = 0f;
            else if (_canvasGroup.alpha == 1)
                Fade(1, 0, 0.2f).Forget();
        }

        private void SetColor(float value)
        {
            if (value > 0.5f)
                _filligBar.color = Color.green;
            else if (value > 0.2f)
                _filligBar.color = Color.yellow;
            else
                _filligBar.color = Color.red;
        }

        private async UniTask Fade(float from, float to, float durationSec)
        {
            DisableInteractable();
            gameObject.SetActive(true);
            var progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime / durationSec;
                _canvasGroup.alpha = Mathf.Lerp(from, to, progress);
                await UniTask.DelayFrame(1);
            }
            EnableInteractable();
        }

        private void EnableInteractable()
        {
            _canvasGroup.interactable = true;
        }

        private void DisableInteractable()
        {
            _canvasGroup.interactable = false;
        }
    }
}