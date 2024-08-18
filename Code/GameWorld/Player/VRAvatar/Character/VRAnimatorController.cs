using System;
using UnityEngine;

public class VRAnimatorController : MonoBehaviour
{
    [SerializeField] private float _speedTreshold = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _smoothing = 0.3f;
    [SerializeField] private LowerBodyIkController _lowerBodyIkController;
    private Animator _animator;

    private Vector3 _previousPos;
    private VRAvatarController _vrRig;
    private int _directionHash;
    private int _speedHash;
    private bool _isInit;

    public void Initialize()
    {
        _animator = GetComponent<Animator>();
        _vrRig = GetComponent<VRAvatarController>();
        _previousPos = _vrRig.Head.vrTarget.position;
        _lowerBodyIkController.Init(_animator);

        _directionHash = Animator.StringToHash("Turn");
        _speedHash = Animator.StringToHash("Forward");

        _isInit = true;
    }

    void Update()
    {
        if (!_isInit) return;

        BodyAnimation();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!_isInit) return;

        _lowerBodyIkController.OnAnimatorIK();
    }

    private void BodyAnimation()
    {
        Vector3 headsetSpeed = (_vrRig.Head.vrTarget.position - _previousPos) / Time.deltaTime;
        headsetSpeed.y = 0;

        Vector3 headsetLocalSpeed = transform.InverseTransformDirection(headsetSpeed);
        _previousPos = _vrRig.Head.vrTarget.position;

        float previousDirection = _animator.GetFloat(_directionHash);
        float previousSpeed = _animator.GetFloat(_speedHash);
        if (headsetLocalSpeed.magnitude < _speedTreshold)
            headsetLocalSpeed = Vector3.zero;

        _animator.SetFloat(_directionHash, Mathf.Lerp(previousDirection, Mathf.Clamp(headsetLocalSpeed.x, -1, 1), _smoothing));
        _animator.SetFloat(_speedHash, Mathf.Lerp(previousSpeed, Mathf.Clamp(headsetLocalSpeed.z, -1, 1), _smoothing));
    }
}
