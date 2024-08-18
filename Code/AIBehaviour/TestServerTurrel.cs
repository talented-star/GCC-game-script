using Cysharp.Threading.Tasks;
using GrabCoin.AIBehaviour;
using GrabCoin.GameWorld.Weapons;
using GrabCoin.Services.Backend.Catalog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TestServerTurrel : MonoBehaviour, IBattleEnemy
{
    [SerializeField] private WeaponBase _weapon;

    [Inject] private CatalogManager _catalogManager;

    private List<IBattleEnemy> _inRangeEnemy = new();
    private IBattleEnemy _targetEnemy;

    public EnemyType GetTypeCreatures => EnemyType.Neutral;

    public Transform GetTransform => transform;

    public GameObject GetGameObject => gameObject;

    public Transform GetEyePoint => transform.GetChild(0);

    public bool IsDie => !gameObject.activeSelf;

    public float GetSize => 0.5f;

    public string GetName => "Dummy";

    void Start()
    {
        _weapon = GetComponent<WeaponBase>();
        CheckShoot();
    }

    private void Update()
    {
        if (_targetEnemy != null)
            transform.LookAt(_targetEnemy.GetTransform);
    }

    private async void CheckShoot()
    {
        while (true)
        {
            await UniTask.WaitForEndOfFrame();
            if (_inRangeEnemy.Count > 0 && _targetEnemy == null)
                _targetEnemy = _inRangeEnemy[0];

            if (_targetEnemy != null)
            {
                await UniTask.WaitForEndOfFrame();
                _weapon.Attack(null, default, this, new());
                await UniTask.Delay((int)(_weapon.CustomData.attackSpeed * 1000f));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = GetComponentInParent<IBattleEnemy>();
        if (enemy != null && !_inRangeEnemy.Contains(enemy))
        {
            _inRangeEnemy.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var enemy = GetComponentInParent<IBattleEnemy>();
        if (enemy != null && _inRangeEnemy.Contains(enemy))
        {
            if (_targetEnemy == enemy)
                _targetEnemy = null;
            _inRangeEnemy.Remove(enemy);
        }
    }
}
