using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(VRAnimatorController))]
public class VRAvatarController : MonoBehaviour
{
    [SerializeField] private MapTransforms _head = new();
    [SerializeField] private MapTransforms _leftHand = new();
    [SerializeField] private MapTransforms _rightHand = new();
    [Space(10)]
    [SerializeField] private Vector3 _cameraHeadOffset;

    private VRAnimatorController _animatorController;
    private RigBuilder _rigBuilder;
    private Transform _eyesTarget;
    private Transform _headBone;
    private Animator _animator;
    private bool _isReady;

    public Vector3 CameraHeadOffset { get => _cameraHeadOffset; set => _cameraHeadOffset = value; }
    public MapTransforms LeftHand => _leftHand;
    public MapTransforms RightHand => _rightHand;
    public MapTransforms Head => _head;
    public bool SimulateMode { get; set; }
    public bool IsReady
    {
        get => _isReady;
        set
        {
            _isReady = value;
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animatorController = GetComponent<VRAnimatorController>();
        _rigBuilder = GetComponent<RigBuilder>();
        _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
    }

    private void Update()
    {
        if (!IsReady) return;

        var eyesToHeadOffset = _eyesTarget.position - _headBone.position;
        var headToRootOffset = _headBone.position - transform.position;
        if (!SimulateMode)
            _head.VRMapping();
        _leftHand.VRMapping();
        _rightHand.VRMapping();

        if (!SimulateMode)
            transform.position = _head.ikTarget.position - eyesToHeadOffset - headToRootOffset;

        transform.forward = Vector3.ProjectOnPlane(_head.vrTarget.forward, Vector3.up).normalized;
        
        if (!SimulateMode)
            _head.VRMapping();
        _leftHand.VRMapping();
        _rightHand.VRMapping();
    }

    public void SetAction(bool isAction)
    {
        enabled = isAction;
        _animatorController.enabled = isAction;
        foreach (var rigLayer in _rigBuilder.layers)
            rigLayer.rig.weight = isAction ? 1 : 0;
    }

    public void SetEyes(Transform eyesTarget)
    {
        this._eyesTarget = eyesTarget;
        if (_animatorController == null)
        {
            _animatorController = GetComponent<VRAnimatorController>();
        }
        _animatorController?.Initialize();
    }

    public void SetVRTargets (Transform vrHead, Transform vrLeftHand, Transform vrRightHand)
    {
        _head.vrTarget = vrHead;
        _leftHand.vrTarget = vrLeftHand;
        _rightHand.vrTarget = vrRightHand;
    }

#if UNITY_EDITOR
    #region "Editor"

    private void OnDrawGizmosSelected()
    {
        Color defaultColor = Gizmos.color;

        Vector3 pointCenter = Head.ikTarget.TransformPoint(Head.trackingOffset.trackingPositionOffset + CameraHeadOffset);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointCenter, 0.04f);

        Gizmos.color = defaultColor;
    }
    #endregion "Editor"
#endif
}
