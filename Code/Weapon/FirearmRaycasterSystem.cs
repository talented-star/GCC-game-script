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
// using static UnityEditor.PlayerSettings;

namespace GrabCoin.GameWorld.Weapons
{
    public class FirearmRaycasterSystem : WeaponBase
    {
        [SerializeField] protected Transform _attackPoint;
        [SerializeField] protected LayerMask _ignoreLayer;

        [Header("Effects")]
        [SerializeField] protected ParticleSystem _muzzleFlash;

        private void Awake()
        {
            countAmmo = SceneNetworkContext.Instance.GetStatistic(Statistics.STATISTIC_AMMO);
            countLaserAmmo = SceneNetworkContext.Instance.GetStatistic(Statistics.STATISTIC_LASER_AMMO);
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
                SceneNetworkContext.Instance.SaveStatistic();
        }

        internal override void Attack(GameObject netIdentity, Action<GameObject, HitbackInfo> callback, IBattleEnemy battleEnemy, AttackData attackData = default)
        {
#if UNITY_SERVER
            Ray rayShot = new Ray(attackData.originPosition, attackData.direction);
            if (Physics.Raycast(rayShot, out var hit, attackData.customData.shootDistance, ~attackData.ignoreMask, QueryTriggerInteraction.Ignore))
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

        protected override void HandleHit(RaycastHit hit)
        {
            if (hit.collider.sharedMaterial != null)
            {
                string materialName = hit.collider.sharedMaterial.name;

                switch (materialName)
                {
                    case "Metal":
                        SpawnDecal(hit, _metalHitEffect.gameObject);
                        break;
                    case "Sand":
                        SpawnDecal(hit, _dustParticle.gameObject);
                        break;
                    case "Stone":
                        SpawnDecal(hit, _dustParticle.gameObject);
                        break;
                    case "WaterFilled":
                        //SpawnDecal(hit, waterLeakEffect);
                        //SpawnDecal(hit, metalHitEffect);
                        break;
                    case "Wood":
                        SpawnDecal(hit, _woodParticle.gameObject);
                        break;
                    case "Meat":
                        //SpawnDecal(hit, fleshHitEffects[Random.Range(0, fleshHitEffects.Length)]);
                        break;
                    case "Character":
                        SpawnDecal(hit, _bloodParticle.gameObject);
                        break;
                    case "WaterFilledExtinguish":
                        //SpawnDecal(hit, waterLeakExtinguishEffect);
                        //SpawnDecal(hit, metalHitEffect);
                        break;
                    default:
                        SpawnDecal(hit, _metalHitEffect.gameObject);
                        break;
                }
            }
            else
            {
                SpawnDecal(hit, _metalHitEffect.gameObject);
            }
        }

        void SpawnDecal(RaycastHit hit, GameObject prefab)
        {
            GameObject spawnedDecal = GameObject.Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
            spawnedDecal.transform.SetParent(hit.collider.transform);
            Destroy(spawnedDecal, 2f);
        }

        private void OnDrawGizmos()
        {
            if (Initialized)
                Gizmos.DrawRay(_attackPoint.position, _attackPoint.forward * 10);
        }
    }
}