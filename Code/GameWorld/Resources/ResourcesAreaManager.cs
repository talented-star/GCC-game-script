using Cysharp.Threading.Tasks;
using GrabCoin.AIBehaviour;
using GrabCoin.Services.Backend.Catalog;
using Mirror;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Resources
{
    public class ResourcesAreaManager : AreaManager
    {
#if UNITY_SERVER
        private CustomSignal _onRefreshData;
        private ResourceStats _resourceStats;
        private GameObject _resourcePrefab;
        private CatalogManager _catalogManager;
        private Transform[] _spawnPoints;
        private string _resourceId;

        [Inject]
        private void Construct(CatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        internal async void Init(ResourceAreaStats areaStats, Transform[] spawnPoints)
        {
            _spawnPoints = spawnPoints;
            _resourcePrefab = areaStats.resourcePrefab;
            _resourceId = areaStats.resourceID;
            if (_catalogManager.GetItemData(areaStats.resourceID) == null)
                await _catalogManager.CashingItem(areaStats.resourceID);

            _resourceStats = JsonConvert.DeserializeObject<ResourceStats>(_catalogManager.GetItemData(areaStats.resourceID).catalogItem.Value.CustomData);

            _onRefreshData = OnRefreshData;
            Translator.Add<GeneralProtocol>(_onRefreshData);

            GetFreePoint(out List<int> freePoint);
            Randomize(freePoint);
            Spawn(_resourceStats.baseCountInArea > _spawnPoints.Length ? _spawnPoints.Length : _resourceStats.baseCountInArea, freePoint);
        }

        private void OnDestroy()
        {
            Translator.Remove<GeneralProtocol>(_onRefreshData);
        }

        private async void OnRefreshData(System.Enum code)
        {
            switch (code)
            {
                case GeneralProtocol.RefreshCatalogData:
                    if (_catalogManager.GetItemData(_resourceId) == null)
                        await _catalogManager.CashingItem(_resourceId);

                    _resourceStats = JsonConvert.DeserializeObject<ResourceStats>(_catalogManager.GetItemData(_resourceId).catalogItem.Value.CustomData);
                    break;
            }
        }

        private async void OnMinedResource(GameObject resource)
        {
            resource.transform.position = Vector3.up * -10000;
            await UniTask.Delay(2000);
            NetworkServer.Destroy(resource);
            await UniTask.Delay(Mathf.Abs(_resourceStats.cooldown * 1000 - 2000));
            GetFreePoint(out List<int> freePoint);
            Randomize(freePoint);
            Spawn(1, freePoint);
        }

        private void GetFreePoint(out List<int> freePoint)
        {
            freePoint = new();
            for (int i = 0; i < _spawnPoints.Length; i++)
                if (_spawnPoints[i].childCount == 0)
                    freePoint.Add(i);
        }

        private void Randomize(List<int> freePoint)
        {
            System.Random random = new System.Random((int)DateTime.UtcNow.Ticks);
            for (int i = freePoint.Count - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                var temp = freePoint[j];
                freePoint[j] = freePoint[i];
                freePoint[i] = temp;
            }
        }

        private void Spawn(int count, List<int> freePoint)
        {
            for (int i = 0; i < count; i++)
            {
                var resource = Instantiate(_resourcePrefab, _spawnPoints[freePoint[i]]);
                resource.GetComponent<MiningResource>().Initialize(_resourceId, _resourceStats, OnMinedResource);

                resource.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                NetworkServer.Spawn(resource);
            }
        }
#endif
    }
}
