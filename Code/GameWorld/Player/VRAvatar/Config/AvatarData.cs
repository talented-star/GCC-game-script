using System;
using UnityEngine;

namespace VRCore.Config
{
    [Serializable]
    public class AvatarData
    {
        public string nameAvatar;
        [TextArea]public string descriptionAvatar;
        public GameObject avatar;
        public GameObject previewAvatar;
    }
}