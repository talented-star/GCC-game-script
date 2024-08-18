using System;
using UnityEngine;

namespace VRCore.Config
{
    [CreateAssetMenu(fileName = "AvatarStorage", menuName = "ScriptableObjects/AvatarStorage")]
    public class AvatarStorage : ScriptableObject
    {
        [SerializeField] private AvatarData[] _avatarDatas;
        private int _indexPreviewAvatar;
        private int _indexSelectedAvatar;

        public event Action<AvatarData> onAvatarChanged;
        public event Action<AvatarData> onAvatarSelected;

        public AvatarData GetAvatarData() =>
            _avatarDatas[_indexSelectedAvatar];

        public void SelectAvatar()
        {
            _indexSelectedAvatar = _indexPreviewAvatar;
            onAvatarSelected?.Invoke(_avatarDatas[_indexSelectedAvatar]);
        }

        public void SetAvatar(int index)
        {
            CheckIndex(ref index);
            _indexPreviewAvatar = index;
            onAvatarChanged?.Invoke(_avatarDatas[index]);
        }

        public void NextAvatar() =>
            SetAvatar(_indexPreviewAvatar + 1);

        public void PrevAvatar() =>
            SetAvatar(_indexPreviewAvatar - 1);

        private void CheckIndex(ref int index)
        {
            if (index == _avatarDatas.Length)
                index = 0;
            if (index < 0)
                index = _avatarDatas.Length - 1;
        }
    }
}