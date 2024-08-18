using UnityEngine;
using System;

namespace GrabCoin.AIBehaviour
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Enemys/Enemy Vision", fileName = "EnemyVision", order = 51)]
    public class EnemyVision : ScriptableObject
    {
        [SerializeField] private VisionParam[] visions;

        public VisionParam[] Visions => visions;

        public EnemyVision(VisionParam param)
        {
            visions = new VisionParam[1];
            visions[0] = param;
        }

        public EnemyVision(VisionParam[] param)
        {
            visions = param;
        }

        public EnemyVision Init(VisionParam[] param)
        {
            visions = param;
            return this;
        }

        private void OnEnable()
        {
            if (visions is null || visions.Length == 0)
            {
                visions = new VisionParam[1];
                visions[0] = new VisionParam { angle = 20, distance = 15, mask = LayerMask.GetMask("Default") };
            }
        }

        private void OnValidate()
        {
            if (visions is null || visions.Length < 1)
            {
                visions = new VisionParam[1];
                visions[0] = new VisionParam { angle = 20, distance = 15, mask = LayerMask.GetMask("Default") };
            }
        }
    }

    [Serializable]
    public struct VisionParam
    {
        public float angle;
        public float distance;
        public LayerMask mask;

        public VisionParam(float ang, float dis, LayerMask mas)
        {
            angle = ang;
            distance = dis;
            mask = mas;
        }
    }
}