using GrabCoin.AIBehaviour;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.UI.HUD;
using InventoryPlus;
using Mirror;
using PlayFab.ClientModels;
using PlayFabCatalog;
using Sources;
using System;
using System.Linq;
using UnityEngine;
using static PlayFabCatalog.AddedCustomDataInPlayFabItems;

namespace GrabCoin.GameWorld.Weapons
{
    public class FirearmThickRaycasterSystem : FirearmRaycasterSystem
    {
        [Header("ThickRaycast")]
        protected float shotRadius = 0.1f;

        internal override void Attack(GameObject netIdentity, Action<GameObject, HitbackInfo> callback, IBattleEnemy battleEnemy, AttackData attackData = default)
        {
#if UNITY_SERVER
            Ray rayShot = new Ray(attackData.originPosition, attackData.direction);
            if (Physics.SphereCast(rayShot, shotRadius, out var hit, attackData.customData.shootDistance, ~attackData.ignoreMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.TryGetComponent(out IAttackable attackable))
                {
                    attackable.Hit(new HitInfo(netIdentity, callback, battleEnemy, attackData.customData));
                }
            }
#else
            if (_weaponHandler != null)
            {
                #region Смерть оптимизации
                var ammoType = _customData.equipmentType switch
                {
                    EquipmentType.Weapon => ConsumableType.Ammo,
                    EquipmentType.LaserWeapon => ConsumableType.LaserAmmo
                };
                var ammos = InventoryData.GetConsumableDatas(ammoType);
                var inventory = InventoryScreenManager.Instance.Inventory;
                _ammoInInventory = 0;
                foreach (var ammo in ammos)
                {
                    var uiSlot = inventory.GetUISlot(ammo.ItemId);
                    if (uiSlot == null) continue;
                    var itemSlot = inventory.GetItemSlot(ammo.ItemId);
                    if (itemSlot == null) continue;

                    _ammoInInventory += itemSlot.GetItemNum();
                }
                #endregion Смерть оптимизации

                if (currentAmmo.Value <= 0)
                {
                    if (_ammoInInventory <= 0) return;
                    CheckAmmo(ammos);
                }
                currentAmmo.Value--;
                Translator.Send(HUDProtocol.CountBullet, new StringData { value = $"{currentAmmo.Value}/{_ammoInInventory}" });
                _attackData.originPosition = _weaponHandler.CameraTransform.position;
                _attackData.direction = _spreadData.GetSpreadAngle(ProceedSpread()) * _weaponHandler.CameraTransform.forward;
                _attackData.ignoreMask = _ignoreLayer;
                _attackData.customData = CustomData;
                Translator.Send(PlayerNetworkProtocol.Attack, _attackData);
                ProceedRecoil();

                if (muzzleArkFlash)
                    muzzleArkFlash.transform.localEulerAngles = Vector3.zero;
                Ray rayShot = new Ray(_attackData.originPosition, _attackData.direction);
                if (Physics.Raycast(rayShot, out var hit, _attackData.customData.shootDistance, ~_attackData.ignoreMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform.TryGetComponent(out IAttackable attackable))
                        Translator.Send(HUDProtocol.SetHealthBarTarget, new HudBarData { target = hit.transform, name = attackable.Owner.GetName });
                    if (muzzleArkFlash)
                        muzzleArkFlash.transform.forward = hit.point - muzzleArkFlash.transform.position;
                }
            }
#endif
            _muzzleFlash?.Play();
            muzzleArkFlash?.LaunchRay();
            HitEffect(_attackData);
            AudioManager.Instance.PlaySound3D("shoot", transform.position);
        }

        private void OnDrawGizmos()
        {
            if (!_attackPoint)
                return;
            Gizmos.DrawWireSphere(_attackPoint.TransformPoint(Vector3.forward * shotRadius / 2f), shotRadius);
        }
    }
}