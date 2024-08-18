using DG.Tweening;
using GrabCoin.GameWorld.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScopeGUIController : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.1f;
    [SerializeField] private Image _scopeImage;
    private CanvasGroup _canvasGroup;
    private Tween fadingTween;
    private bool scoping = false;

    private void Start()
    {
        _canvasGroup=GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    public void SetScoped(bool scoped)
    {
        if(scoped == scoping)
        {
            return;
        }
        if(fadingTween?.IsActive()??false)
        {
            fadingTween.Kill();
            fadingTween = null;
        }
        fadingTween = DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, scoped ? 1f : 0f, _fadeDuration).SetEase(Ease.InCirc);
        scoping = scoped;
    }
}
