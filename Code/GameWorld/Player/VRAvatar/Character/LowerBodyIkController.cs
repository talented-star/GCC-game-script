using System;
using UnityEngine;

[Serializable]
public class LowerBodyIkController
{
    [SerializeField] private Vector3 _footOffset;

    [SerializeField] private Vector3 _raycastLeftOffset;
    [SerializeField] private Vector3 _raycastRightOffset;

    private Animator _animator;
    private int _leftFootWeightHash;
    private int _rightFootWeightHash;

    public void Init(Animator animator)
    {
        this._animator = animator;

        _leftFootWeightHash = Animator.StringToHash("LeftFoot");
        _rightFootWeightHash = Animator.StringToHash("RightFoot");
    }

    public void OnAnimatorIK()
    {
        Vector3 leftFootPosition = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        Vector3 rightFootPosition = _animator.GetIKPosition(AvatarIKGoal.RightFoot);

        bool isLeftFootDown = Physics.Raycast(leftFootPosition + _raycastLeftOffset, Vector3.down, out var hitLeftFoot);
        bool isRightFootDown = Physics.Raycast(rightFootPosition + _raycastRightOffset, Vector3.down, out var hitRightFoot);

        CalculateFoot(isLeftFootDown, hitLeftFoot, AvatarIKGoal.LeftFoot);
        CalculateFoot(isRightFootDown, hitRightFoot, AvatarIKGoal.RightFoot);
    }

    private void CalculateFoot(bool isFootDown, RaycastHit hitFoot, AvatarIKGoal iKGoal)
    {
        if (isFootDown)
        {
            var footWeight = _animator.GetFloat(iKGoal == AvatarIKGoal.RightFoot ? _rightFootWeightHash : _leftFootWeightHash);
            _animator.SetIKPositionWeight(iKGoal, footWeight);
            _animator.SetIKPosition(iKGoal, hitFoot.point + _footOffset);

            Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(_animator.transform.forward, hitFoot.normal), hitFoot.normal);
            _animator.SetIKRotationWeight(iKGoal, footWeight);
            _animator.SetIKRotation(iKGoal, footRotation);
        }
        else
        {
            _animator.SetIKPositionWeight(iKGoal, 0);
            _animator.SetIKRotationWeight(iKGoal, 0);
        }
    }
}
