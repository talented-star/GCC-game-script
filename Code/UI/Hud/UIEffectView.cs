using System;
using Common;
using Cysharp.Threading.Tasks;
using PureAnimator;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI.Resources
{
    public class UIEffectView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_FontAsset _normalFontAsset;
        [SerializeField] private TMP_FontAsset _critFontAsset;
        [SerializeField] private float _duration = 0.4f;

        //private ResourceDropperConfig dropperConfig;
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Vector2 offset;
        private float _prevFontSize;
        private FontStyles _prevFontStyle;
        private Color _prevColor;

        public void Init(/*ResourceDropperConfig dropperConfig*/)
        {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _prevFontSize = _damageText.fontSize;
            _prevFontStyle = _damageText.fontStyle;
            _prevColor = _damageText.color;
        }

        public void Animate(bool isCrit, string damage)
        {
            _rect.position = Camera.main.ViewportToScreenPoint(Vector3.zero);
            _damageText.fontSize = isCrit ? _prevFontSize + 5 : _prevFontSize;
            _damageText.fontStyle = isCrit ? FontStyles.Italic : _prevFontStyle;
            _damageText.font = isCrit ? _critFontAsset : _normalFontAsset;
            _damageText.color = isCrit ? Color.red : _prevColor;
            _damageText.outlineWidth = isCrit ? 0.15f : 0.05f;
            _damageText.text = damage;
            if (isCrit)
                _damageText.text += "\nCrit!";

            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 0f);
            Vector2 circle = new Vector2(x, y).normalized;
            Vector2 downOffset = Vector2.down * 200f;
            offset = downOffset + circle * 100f;

            StartJump();
        }

        private void StartJump()
        {
            _canvasGroup.alpha = 1f;
            var startPosition = Vector2.zero;
            //var pureAnimatorJump = Services<PureAnimatorController>.Get().GetPureAnimator();
            //pureAnimatorJump.Play(_duration, JumpCommand, () => { });

            var pureAnimatorMove = Services<PureAnimatorController>.Get().GetPureAnimator();
            pureAnimatorMove.Play(_duration,
                progress =>
                {
                    _rect.anchoredPosition = Vector2.Lerp(startPosition, startPosition + offset, progress);
                    //+ new Vector2(pureAnimatorJump.LastChanges.Value.x, pureAnimatorJump.LastChanges.Value.y);// pureAnimatorJump.LastChanges.Value*/;
                    //_rect.position = Vector3.Lerp(startPosition,
                    //    new Vector3(startPosition.x + offset.x, startPosition.y + offset.y, 0), progress) + pureAnimatorJump.LastChanges.Value;

                    //transform.position = Vector3.Lerp(startPosition, startPosition + offset,
                    //    progress) + pureAnimatorJump.LastChanges.Value;
                    return default;
                },
                StartFadeAlpha
            );
        }

        private void StartFadeAlpha()
        {
            var pureAnimatorJump = Services<PureAnimatorController>.Get().GetPureAnimator();
            pureAnimatorJump.Play(_duration, AlphaCommand, ExitCommand);
        }

        private TransformChanges JumpCommand(float progress)
        {
            if (progress > 1) progress = 1;
            Vector3 position =
                Vector3.Scale(new Vector3(0, 100f * DemoTestSpawner.Curve.Evaluate(progress), 0),
                    Vector3.up);
            return new TransformChanges(position);
        }

        private TransformChanges AlphaCommand(float progress)
        {
            if (progress > 1) progress = 1;
            _canvasGroup.alpha = 1 - progress;
            return default;
        }

        private void ExitCommand()
        {
            _rect.gameObject.SetActive(false);
        }
    }
}