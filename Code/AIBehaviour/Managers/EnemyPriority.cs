using System;

namespace GrabCoin.AIBehaviour
{
    [Serializable]
    public struct EnemyPriority : IComparable
    {
        public IBattleEnemy enemy;
        public float damage;
        public float threatReduction;

        public EnemyPriority(IBattleEnemy enemy, float damage = 0, float threatReduction = 0)
        {
            this.enemy = enemy;
            this.damage = damage;
            this.threatReduction = threatReduction;
        }

        public void Damage(int damage)
        {
            this.damage += damage;
            threatReduction = 0;
        }

        public int CompareTo(object obj)
        {
            EnemyPriority ep = (EnemyPriority)obj;
            return ep.damage.CompareTo(damage);
        }
    }
}
