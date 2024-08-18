using GrabCoin.GameWorld.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.GameWorld
{

    public class UnitAimer : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private ThirdPersonCameraController _cameraController;
        [SerializeField] private UnitMotor _unitMotor;

        [Header("Aiming")]
        [SerializeField, Range(0f, 1f)] private float _aimingWeight = 0.85f;
        [SerializeField, Range(0f, 1f)] private float _aimingBodyWeight = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _aimingHeadWeight = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _aimingClampWeight = 0.65f;

        private Animator _animator;

        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            //_animator.SetLookAtPosition(_cameraController.transform.forward * 100f * _animator.humanScale + _cameraController.transform.position);
            //if (_unitMotor.isAiming)
            //    _animator.SetLookAtWeight(_aimingWeight, _aimingBodyWeight, _aimingHeadWeight, 0f, _aimingClampWeight);
        }
    }
}