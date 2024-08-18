using GrabCoin.AIBehaviour;
using UnityEngine;
using UnityEngine.Events;

namespace GrabCoin.GameWorld.Weapons
{
    public class Hittable : MonoBehaviour, IAttackable
    {
        [SerializeField] protected BodyPart _bodyPart = BodyPart.Stomach;
        [SerializeField] protected HittableInfo _hittableInfo;

        public IBattleEnemy Owner { get; set; }

        public event System.Action<HitInfo, BodyPart> hitCallback;

        public virtual void Hit(HitInfo hitInfo)
        {
            //hitInfo.shooterCallback?.Invoke(new HitbackInfo(_bodyPart));
            hitCallback?.Invoke(hitInfo, _bodyPart);
        }
    }
}