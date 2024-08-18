using GrabCoin.GameWorld.Player;
using GrabCoin.GameWorld.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAimer : MonoBehaviour
{
    [SerializeField] private ThirdPersonPlayerController _playerController;
    [SerializeField] private ThirdPersonCameraController _cameraController;
    [SerializeField] private Animator _animator;
    [SerializeField] private Vector3 _rotationOffset = new Vector3(0, 0, -90f);
    [SerializeField] private LayerMask _ignoreLayer;
    [SerializeField] private bool _constantAim = false;
    private Transform _aimTarget;
    private WeaponHandler _weaponHandler;

    public void Initialize(WeaponHandler weaponHandler)
    {
        _weaponHandler = weaponHandler;
    }

    private void Start()
    {
        _aimTarget = new GameObject("aim target").transform;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_playerController.UnitMotor.isAiming || _constantAim)
        {
            var rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            var leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);

            Ray targetRay = new Ray(_cameraController.transform.position, _cameraController.transform.forward);
            _aimTarget.position = targetRay.GetPoint(100f);

            Vector3 handForward = rightHand.forward;
            Vector3 targetForward = (_aimTarget.position - rightHand.position).normalized;

            Quaternion targetRotation = Quaternion.FromToRotation(handForward, targetForward) * rightHand.rotation;
        }


    }

    private void LateUpdate()
    {
    }
}
