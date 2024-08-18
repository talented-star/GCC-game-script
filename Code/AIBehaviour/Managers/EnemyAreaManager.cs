using Cysharp.Threading.Tasks;
using GrabCoin.Config;
using GrabCoin.GameWorld;
using GrabCoin.Services.Backend.Catalog;
using Mirror;
using NaughtyAttributes;
using OccaSoftware.SuperSimpleSkybox.Runtime;
using PlayFab.EconomyModels;
using PlayFabCatalog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GrabCoin.AIBehaviour
{
    public class EnemyAreaManager : AreaManager
    {
        [SerializeField] private EnemyItem _defaultEnemyStats;
        [Range(50, 1000)]
        [SerializeField] private float _patrulDistance = 50f;
        [Range(50, 1000)]
        [SerializeField] private float _pursuitDistance = 80f;
        [SerializeField] private float _reSpawnInterval;
        [SerializeField] private int _maxEnemys;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Sun _dayNight;
        [SerializeField] private List<EnemyBehaviour> _enemys = new();

        private static int _enemyId;

        private EnemyFactory _spawner;
        private CatalogManager _catalogManager;
        private EnemyBehaviours _enemyBehaviours;

        [Space(20)]
        [SerializeField] private bool _isNightCreatures;
        private CustomSignal _onRefreshData;

        private bool IsNight => Vector3.Dot(Vector3.up, _dayNight.transform.up) < 0;

        [Inject]
        private void Construct(EnemyFactory factory, CatalogManager catalogManager, EnemyBehaviours enemyBehaviours)
        {
            _spawner = factory;
            _catalogManager = catalogManager;
            _enemyBehaviours = enemyBehaviours;
        }

        public async void Init(AreaStats areaStats)
        {
#if UNITY_SERVER
            if (!ScenePortConfig.isEnemyActive[areaStats.enemyKey])
            {
                gameObject.SetActive(false);
                return;
            }
#endif
            await _catalogManager.WaitInitialize();
            _defaultEnemyStats = _catalogManager.GetEnemyData(areaStats.enemyKey);
            _patrulDistance = areaStats._patrulDistance;
            _pursuitDistance = areaStats._pursuitDistance;
            _reSpawnInterval = areaStats._reSpawnInterval;
            _maxEnemys = areaStats._maxEnemys;
            _groundMask = areaStats._groundMask;

            _defaultEnemyStats.customData.moveDistance = _patrulDistance;
            _defaultEnemyStats.behaviour = _enemyBehaviours.GetBehaviour(_defaultEnemyStats.customData.enemyType);

            while (_enemys.Count < _maxEnemys)
            {
                FistWawe();

                //_isFirstWave = true;
                await UniTask.DelayFrame(1);
            }

            _onRefreshData = OnRefreshData;
            Translator.Add<GeneralProtocol>(_onRefreshData);
        }

        private void OnDestroy()
        {
            Translator.Remove<GeneralProtocol>(_onRefreshData);
        }

        private void OnRefreshData(System.Enum code)
        {
            switch (code)
            {
                case GeneralProtocol.RefreshCatalogData:
                    _defaultEnemyStats = _catalogManager.GetEnemyData(_defaultEnemyStats.ItemId);
                    foreach (var enemy in _enemys)
                    {
                        enemy.RefreshStats(_defaultEnemyStats);
                    }
                    break;
            }
        }

        public override void AreaUpdate()
        {
#if UNITY_SERVER
            //if (!_isNightCreatures || IsNight)
            {
                if (_enemys.Count > 0)
                {
                    for (int i = 0; i < _enemys.Count; i++)
                    {
                        //if (!_enemys[i].IsEnemyDeath && !_enemys[i].isEnabled)
                        //    _enemys[i].EnemySetActive(true);
                        //if (_enemys[i].isEnabled)
                        //    _enemys[i].EnemyUpdate();

                        if (_enemys[i].isEnabled)
                        {
                            if (_enemys[i].IsAlarm && Vector3.Distance(transform.position, _enemys[i].transform.position) > _pursuitDistance)
                            {
                                _enemys[i].ReturnHomeArea();
                            }
                        }
                    }
                }
            }
            //else
            //{
            //    for (int i = 0; i < _enemys.Count; i++)
            //    {
            //        if (_enemys[i].isEnabled) _enemys[i].EnemySetActive(false);
            //    }
            //}
#endif
        }

        //public override void AreaFixedUpdate()
        //{
        //    foreach (BaseEnemy enemy in _enemys)
        //    {
        //        if (enemy.gameObject.activeSelf)
        //            enemy.EnemyFixedUpdate();
        //    }
        //}

        //public override void AreaLateUpdate()
        //{
        //    foreach (BaseEnemy enemy in _enemys)
        //    {
        //        if (enemy.gameObject.activeSelf)
        //            enemy.EnemyLateUpdate();
        //    }
        //}

        public void OnEnemyDeath(BaseEnemy enemy)
        {
            for (int i = 0; i < _enemys.Count; i++)
            {
                if (enemy == _enemys[i])
                {
                    StartCoroutine(OnReSpawn(enemy));
                    break;
                }
            }
        }

        private IEnumerator OnReSpawn(BaseEnemy enemy)
        {
            yield return new WaitForSeconds(_reSpawnInterval);

            enemy.transform.position = transform.position;
            enemy.transform.rotation = Quaternion.identity;

            if (_defaultEnemyStats.behaviour == null)
                _defaultEnemyStats.behaviour = _enemyBehaviours.GetBehaviour(_defaultEnemyStats.customData.enemyType);
            enemy.EnemyRespawn(_defaultEnemyStats);
        }

        private void FistWawe()
        {
            EnemyBehaviour enemy = _spawner.Create(this, _defaultEnemyStats, transform.position, Quaternion.identity, transform.position);
            NetworkServer.Spawn(enemy.gameObject);
            StartCoroutine(enemy.EEnemyInitilization(_catalogManager, _defaultEnemyStats, transform.position, default));
            enemy.Agent.avoidancePriority = Random.Range(0, 100);
            _enemys.Add(enemy);
            enemy.nameEnemy = _defaultEnemyStats.DisplayName;
            //enemy.nameEnemy = enemy.gameObject.name += $"-{_enemyId}";
            enemy.gameObject.name = enemy.nameEnemy;

            _enemyId++;
            enemy.EnemySetActive(true, true);
        }

#if UNITY_EDITOR
        private enum ShowState
        {
            None,
            ShowEyesight,
            ShowAttack
        }
        private ShowState showEyesight;

        [SerializeField] private bool _showEyesight;
        [SerializeField] private bool _showAttack;

        protected void OnValidate()
        {
            foreach (var enemy in _enemys)
                enemy.HideEnemyEyesight();
            if (_showEyesight)
                foreach (var enemy in _enemys)
                    enemy.ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_defaultEnemyStats.customData.enemyEyesight.ToArray()));
            if (_showAttack)
                foreach (var enemy in _enemys)
                    enemy.ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_defaultEnemyStats.customData.attackZone.ToArray()));
        }

        [ShowIf("showEyesight", ShowState.None)]
        [Button("Show eyesight")]
        public void ShowEyesight()
        {
            showEyesight = ShowState.ShowEyesight;
            foreach (var enemy in _enemys)
                enemy.ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_defaultEnemyStats.customData.enemyEyesight.ToArray()));
        }

        [ShowIf("showEyesight", ShowState.ShowEyesight)]
        [Button("Hide eyesight")]
        private void HideEyesight()
        {
            showEyesight = ShowState.None;
            foreach (var enemy in _enemys)
                enemy.HideEnemyEyesight();
        }

        [ShowIf("showEyesight", ShowState.None)]
        [Button("Show attack")]
        private void ShowAttack()
        {
            showEyesight = ShowState.ShowAttack;
            foreach (var enemy in _enemys)
                enemy.ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_defaultEnemyStats.customData.attackZone.ToArray()));
        }

        [ShowIf("showEyesight", ShowState.ShowAttack)]
        [Button("Hide attack")]
        private void HideAttack()
        {
            showEyesight = ShowState.None;
            foreach (var enemy in _enemys)
                enemy.HideEnemyEyesight();
        }
#endif
    }
}
