using UnityEngine;

public class TestScaleAnimation : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector3 startScale = Vector3.one;
    [SerializeField] private Vector3 endScale;
    [SerializeField] private float duration;
    [SerializeField] private float delay;
    [SerializeField] private AnimationCurve curve;

    private float currentTime = 0f;
    private float t = 0f;
    private float currentDelay = 0f;

    private void Update()
    {
        if (currentDelay >= delay)
        {
            Animate();
        }
        else
        {
            currentDelay += Time.deltaTime;
        }
    }

    private void Animate()
    {
        if (currentTime >= duration)
        {
            currentTime = 0f;
            transform.SetAsFirstSibling();
        }

        t = currentTime / duration;

        targetTransform.localScale = Vector3.Lerp(startScale, endScale, curve.Evaluate(t));
        canvasGroup.alpha = 1f - t;

        currentTime += Time.deltaTime;
    }
}
