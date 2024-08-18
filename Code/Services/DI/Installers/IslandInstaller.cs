using GrabCoin.AIBehaviour;
using GrabCoin.GameWorld.Resources;
using Mirror;
using PlayFabCatalog;
using UnityEngine;

namespace Assets.Scripts.Code.Services.DI.Installers
{
    public class IslandInstaller : WorldInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            InstallFactory();
            InstallServer();
        }

        [Server]
        private void InstallServer()
        {
            var enemyManager = new GameObject("GlobalEnemyManager");
            var instance = Container.InstantiateComponent(typeof(GlobalEnemyManager), enemyManager);
            Container.Bind<GlobalEnemyManager>().FromInstance(instance as GlobalEnemyManager).AsSingle().NonLazy();

            var resourcesManager = new GameObject("GlobalResourcesManager");
            instance = Container.InstantiateComponent(typeof(GlobalResourcesManager), enemyManager);
            Container.Bind<GlobalResourcesManager>().FromInstance(instance as GlobalResourcesManager).AsSingle().NonLazy();
        }

        private void InstallFactory()
        {
            Container.BindFactory<EnemyAreaManager, EnemyItem, Vector3, Quaternion, Vector3, EnemyBehaviour, EnemyFactory>().FromFactory<FactoryEnemy>();
        }
    }
}