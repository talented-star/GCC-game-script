using UnityEngine;

namespace VRCore.Editor
{
    public class HelperController : MonoBehaviour
    {
        [SerializeField] private VRAvatarController _avatarController;
        [SerializeField] private Transform _vrHeadHelper;
        [SerializeField] private Transform _eyePoint;
        [SerializeField] private Transform _vrLHandHelper;
        [SerializeField] private Transform _vrRHandHelper;

        private void Awake()
        {
            InsertHelpers();
        }

        private void InsertHelpers()
        {
            _avatarController.Head.vrTarget = _vrHeadHelper;
            _avatarController.LeftHand.vrTarget = _vrLHandHelper;
            _avatarController.RightHand.vrTarget = _vrRHandHelper;

            RecalculateCameraOffset();
        }

        private void RecalculateCameraOffset()
        {
            Animator animator = _avatarController.GetComponent<Animator>();
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);

            _eyePoint.transform.SetParent(head);
            _eyePoint.transform.localPosition = _avatarController.CameraHeadOffset;

            _avatarController.SetEyes(_eyePoint.transform);
            _avatarController.IsReady = true;
        }
    }
}