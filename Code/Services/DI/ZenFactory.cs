using GrabCoin.AIBehaviour;
using GrabCoin.GameWorld.Player;
using PlayFabCatalog;
using UnityEngine;
using Zenject;

public class ZenFactory<T> : IFactory<GameObject, T> where T : MonoBehaviour
{
    protected readonly DiContainer container;

    public ZenFactory(DiContainer container)
    {
        this.container = container;
    }

    public virtual T Create(GameObject prefab)
    {
        return container.InstantiatePrefabForComponent<T>(prefab);
    }
}

public class Factory<T> : PlaceholderFactory<GameObject, T>
{

}

public class EnemyFactory : PlaceholderFactory<EnemyAreaManager, EnemyItem, Vector3, Quaternion, Vector3, EnemyBehaviour>
{

}

public class FactoryEnemy : ZenFactory<EnemyBehaviour>, IFactory<EnemyAreaManager, EnemyItem, Vector3, Quaternion, Vector3, EnemyBehaviour>
{
    public FactoryEnemy(DiContainer container) : base(container)
    {
    }

    public EnemyBehaviour Create(EnemyAreaManager enemyManager, EnemyItem stats, Vector3 pos, Quaternion rot, Vector3 homePoseition)
    {
        var enemy = base.Create(stats.itemConfig.Prefab);
        enemy.transform.position = pos;
        enemy.transform.rotation = rot;

        EnemyBehaviour behaviour = enemy.GetComponentInChildren<EnemyBehaviour>();
        if (behaviour != null)
        {
            behaviour.EnemyInit(stats, homePoseition);
            behaviour._deathEnemy += enemyManager.OnEnemyDeath;
            return behaviour;
        }
        
        return enemy;
    }
}