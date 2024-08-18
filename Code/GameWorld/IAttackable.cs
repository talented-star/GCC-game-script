using GrabCoin.AIBehaviour;
using UnityEngine;

namespace GrabCoin.GameWorld
{
    public interface IAttackable
    {
        public IBattleEnemy Owner { get; set; }

        public void Hit(HitInfo hitInfo);
    }
}