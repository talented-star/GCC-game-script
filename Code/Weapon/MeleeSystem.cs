using GrabCoin.AIBehaviour;
using GrabCoin.GameWorld;
using GrabCoin.GameWorld.Weapons;
using GrabCoin.Services.Backend.Catalog;
using Mirror;
using PlayFabCatalog;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : WeaponBase
{
    [SerializeField] protected float _attackRadius = 3;
    [SerializeField] protected Vector3 _attackPointOffset = Vector3.zero;
    [SerializeField] protected LayerMask _ignoreLayer;

    private List<IAttackable> _colliders = new();

    internal override void Attack(GameObject netIdentity, Action<GameObject, HitbackInfo> callback, IBattleEnemy battleEnemy, AttackData attackData = default)
    {
        FindEnemyes(attackData.attackParams, battleEnemy);
        
        for (int i = 0; i < _colliders.Count; i++)
        {
            if (CustomData == null)
            {
                var inventory = InventoryScreenManager.Instance.Inventory;
                var itemIndex = inventory.GetItemIndex(inventory.hotbarUISlots[0]);
                var tmpitem = inventory?.GetItemSlot(itemIndex)?.GetItemType();
                CustomData = (_catalogManager.GetItemData(tmpitem.itemID) as EquipmentItem).customData;
            }
            _colliders[i].Hit(new HitInfo(netIdentity, callback, battleEnemy, CustomData));
        }
    }

    public void FindEnemyes(List<VisionParam> attackParams, IBattleEnemy battleEnemy)
    {
        Queue<Collider> copyList =
            new Queue<Collider>(Physics.OverlapSphere(transform.position, attackParams[0].distance, attackParams[0].mask, QueryTriggerInteraction.Collide));

        _colliders.Clear();
        while (copyList.Count > 0)
        {
            Collider coll = copyList.Dequeue();
            if (!coll.TryGetComponent(out IAttackable attackable))
                continue;
            if (!coll.TryGetComponent(out IBattleEnemy creature))
                creature = coll.GetComponentInParent<IBattleEnemy>();

            if (creature != null && !creature.Equals(battleEnemy) && !creature.IsDie)
                if (creature.GetTypeCreatures != battleEnemy.GetTypeCreatures)
                    if (Vision.IsVisibleUnit(creature, battleEnemy.GetEyePoint, creature.GetTransform, ScriptableObject.CreateInstance<EnemyVision>().Init(attackParams.ToArray())))
                        _colliders.Add(attackable);
        }
    }

#if UNITY_EDITOR
    public bool isDrawGizmos;
    private void OnDrawGizmos()
    {
        if (!isDrawGizmos) return;
        Gizmos.DrawCube(transform.TransformPoint(_attackPointOffset), new Vector3(_attackRadius, 5f, _attackRadius));
    }
#endif
}
