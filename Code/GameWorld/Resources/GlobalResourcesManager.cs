using GrabCoin.Config;
using GrabCoin.Services.Backend.Catalog;
using System.Linq;
using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Resources
{
    public class GlobalResourcesManager : GlobalManager
    {
#if UNITY_SERVER
        private CatalogManager _catalogManager;

        [Inject]
        private void Construct(CatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        private async void Start()
        {
#if UNITY_SERVER
            if (!ScenePortConfig.isResources)
            {
                gameObject.SetActive(false);
                return;
            }
#endif

            var enemyAreas = Translator.SendAnswers<AreaManagerProtocol, IntData, ObjectData>(AreaManagerProtocol.FindResourcesAreaPoints, new IntData())
                .Select(obj => obj.value as ResourcesAreaPoint)
                .ToList();

            await _catalogManager.WaitInitialize();

            foreach (ResourcesAreaPoint point in enemyAreas)
            {
                var newObject = _container.InstantiateComponent(typeof(ResourcesAreaManager), point.gameObject);
                var area = newObject as ResourcesAreaManager;
                area.Init(point.AreaStats, point.SpawnPoints);

                _areas.Add(area);
            }

            Debug.Log($"Init Enemy Manager. Count area: {_areas.Count}");
        }
#endif
    }
}
