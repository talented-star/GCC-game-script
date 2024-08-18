using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    public interface IBattleEnemy
    {
        //float Damage(IBattleEnemy enemy, int damage);

        string GetName { get; }

        EnemyType GetTypeCreatures { get; }

        Transform GetTransform { get; }

        GameObject GetGameObject { get; }

        Transform GetEyePoint { get; }

        bool IsDie { get; }

        float GetSize { get; }
    }
}