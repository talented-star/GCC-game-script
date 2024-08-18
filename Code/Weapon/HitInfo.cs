using GrabCoin.AIBehaviour;
using PlayFabCatalog;
using System;
using UnityEngine;

public class HitInfo
{
    public EquipmentCustomData customData;
    public IBattleEnemy battleEnemy;
    public Action<GameObject, HitbackInfo> shooterCallback;
    public GameObject netIdentity;

    public HitInfo(GameObject netIdentity, Action<GameObject, HitbackInfo> weaponSystem, IBattleEnemy battleEnemy, EquipmentCustomData customData)
    {
        this.netIdentity = netIdentity;
        shooterCallback = weaponSystem;
        this.battleEnemy = battleEnemy;
        this.customData = customData;
    }
}
