using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using VRCore.Input;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CharacterMoveHelper : MonoBehaviour
{
    [SerializeField] private Transform characterModel;

    private XROrigin _xrOrigin;
    private CapsuleCollider _collider;
    private Animator _animator;
    private VRAvatarController _controller;
    private GameObject eyePoint;

    void Awake()
    {
        _xrOrigin = GetComponent<XROrigin>();
        _collider = GetComponent<CapsuleCollider>();
        eyePoint = new GameObject("Eyes");
    }

    private IEnumerator Start()
    {
        _controller = characterModel.GetComponent<VRAvatarController>();
        _controller.SetEyes(eyePoint.transform);
        yield return new WaitForSeconds(1f);

        RecalculateCameraOffset();
    }

    private void Update()
    {
        UpdateCharacterController();
    }

    public void SetCharacter(VRAvatarController newCharacter)
    {
        Destroy(characterModel.gameObject);
        characterModel = newCharacter.transform;
        characterModel.position = _xrOrigin.transform.position;
        _controller = newCharacter;
        RecalculateCameraOffset();
    }

    private void RecalculateCameraOffset()
    {
        _animator = characterModel.GetComponent<Animator>();
        Transform head = _animator.GetBoneTransform(HumanBodyBones.Head);

        eyePoint.transform.SetParent(head);
        eyePoint.transform.localPosition = _controller.CameraHeadOffset;

        float headHeight = head.position.y - _animator.transform.position.y;
        float sumHeight = headHeight + _controller.CameraHeadOffset.y;

        _xrOrigin.CameraYOffset = sumHeight - _xrOrigin.Camera.transform.localPosition.y;

        _controller.SetEyes(eyePoint.transform);
        _controller.IsReady = true;
    }

    protected virtual void UpdateCharacterController()
    {
        if (_xrOrigin == null || _collider == null)
            return;

        var height = _xrOrigin.CameraInOriginSpaceHeight;//TO DO: old - Mathf.Clamp(_xrOrigin.CameraInOriginSpaceHeight, _controllerDriver.minHeight, _controllerDriver.maxHeight);

        Vector3 center = _xrOrigin.CameraInOriginSpacePos;
        center.y = height / 2f;// + _collider.skinWidth;

        _collider.height = height;
        _collider.center = center;
    }

    private void CheckPressTrigger(InputAction.CallbackContext callback)
    {
        RecalculateCameraOffset();
    }
}
