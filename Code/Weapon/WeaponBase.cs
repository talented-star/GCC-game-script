using GrabCoin.AIBehaviour;
using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using GrabCoin.UI.HUD;
using InventoryPlus;
using Mirror;
using PlayFab.ClientModels;
using PlayFabCatalog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using static PlayFabCatalog.AddedCustomDataInPlayFabItems;

namespace GrabCoin.GameWorld.Weapons
{
    [Serializable]
    public struct AttackData : ISendData
    {
        public Vector3 originPosition;
        public Vector3 direction;
        public Vector3[] directions;
        public float radius;
        public LayerMask ignoreMask;
        public EquipmentCustomData customData;

        public bool isSingle;
        public List<VisionParam> attackParams;
    }

    public enum AttackZoneType
    {
        Single,
        Area
    }

    public enum AimGUIType
    {
        None,
        AimCross,
        Scope
    }

    [RequireComponent(typeof(NetworkIdentity))]
    public abstract class WeaponBase : NetworkBehaviour
    {
        [Header("Base")]
        //[SerializeField] protected private AttackInfo attackInfo = new AttackInfo();
        [SerializeField] protected float _attackDelay;
        [SerializeField] protected bool _attackOnClick;
        [SerializeField] protected ArcReactor_Launcher muzzleArkFlash;
        [SerializeField] protected ParticleSystem _bloodParticle;
        [SerializeField] protected ParticleSystem _dustParticle;
        [SerializeField] protected ParticleSystem _woodParticle;
        [SerializeField] protected ParticleSystem _metalHitEffect;
        [SerializeField] protected AnimatorOverrideController _animatorOverrideController;
        [SerializeField] protected AttackZoneType zoneType;
        [SerializeField] protected AimGUIType aimGUIType = AimGUIType.AimCross;
        [SerializeField] protected float _aimingZoomAmount = 1.2f;

        [SerializeField] protected Transform _followPointLeftHand;

        [Header("Recoil")]
        [SerializeField] protected RecoilData _recoilData;
        [SerializeField] protected float _recoilAcceleration = 0.1f;
        [SerializeField] protected float _recoilDeceleration = 1f;
        [Header("Spread")]
        [SerializeField] protected SpreadData _spreadData;

        public EquipmentCustomData CustomData
        {
            get { return _customData; }
            set { _customData = value; }
        }

        public AnimatorOverrideController AnimatorOverrideController
        {
            get { return _animatorOverrideController; }
        }

        public AimGUIType AimGUIType { get => aimGUIType; }

        protected EquipmentCustomData _customData;
        protected CatalogManager _catalogManager;
        protected InventoryDataManager _inventoryData;
        protected float _attackTimer = 0;
        protected bool _availableForAttack = false;
        protected IWeaponHandler _weaponHandler;
        protected AttackData _attackData;
        protected ThirdPersonCameraController _thirdPersonCamera;
        protected Vector3ArrayData _weaponHandData;
        protected StatisticValue countAmmo;
        protected StatisticValue countLaserAmmo;
        protected StatisticValue currentAmmo;
        protected string _ammoId;
        protected int _ammoInInventory;
        [SerializeField] protected float attackAccelerationTime = 0f;

        //private int countBullet;

        public bool Initialized
        {
            get { return _weaponHandler != null; }
        }

        public float AimingZoomAmount
        {
            get { return _aimingZoomAmount; }
        }

        protected InventoryDataManager InventoryData
        {
            get
            {
                if (_inventoryData == null)
                    _inventoryData = InventoryScreenManager.Instance.InventoryDataManager;
                return _inventoryData;
            }
        }

        internal abstract void Attack(GameObject netIdentity, Action<GameObject, HitbackInfo> callback, IBattleEnemy battleEnemy, AttackData attackData = default);

        [Inject]
        private void Construct(CatalogManager catalogManager, InventoryDataManager inventoryData)
        {
            _catalogManager = catalogManager;
            _inventoryData = inventoryData;
            _weaponHandData = new Vector3ArrayData { value = new[] { Vector3.zero, Vector3.zero, Vector3.zero } };
        }

        public void Initialize(IWeaponHandler weaponHandler, EquipmentCustomData customData)
        {
            _weaponHandler = weaponHandler;
            _customData = customData;

            var ammoType = _customData.equipmentType switch
            {
                EquipmentType.Weapon => ConsumableType.Ammo,
                EquipmentType.LaserWeapon => ConsumableType.LaserAmmo
            };
            currentAmmo = ammoType switch
            {
                ConsumableType.Ammo => countAmmo,
                ConsumableType.LaserAmmo => countLaserAmmo
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
            Translator.Send(HUDProtocol.CountBullet, new StringData { value = $"{currentAmmo?.Value ?? 0}/{_ammoInInventory}" });

            if (!_weaponHandler.CameraTransform.TryGetComponent(out _thirdPersonCamera))
            {
                Debug.LogError("ThirdPersonCameraController not set!");
            }
        }

        protected virtual void Update()
        {
            TimerLogic();
            InputLogic();
            SendHandFollow();
        }

        public void SetAvailableForAttack(bool available)
        {
            _availableForAttack = available;
        }

        public bool GetAvailableForAttack()
        {
            return _availableForAttack;
        }

        protected virtual void TimerLogic()
        {
            _attackTimer += Time.deltaTime;
        }

        protected virtual IEnumerator ProceedAttack()
        {
            if (_attackDelay > 0)
                yield return new WaitForSeconds(_attackDelay);
            Attack(null, default, default);
        }

        public void ProceedRecoil()
        {
            if (_thirdPersonCamera)
            {
                attackAccelerationTime = Mathf.Clamp01(attackAccelerationTime + _recoilAcceleration);
                _thirdPersonCamera.ProcessRecoil(_recoilData.Process(attackAccelerationTime));
            }
        }

        public Vector2 ProceedSpread()
        {
            return _spreadData.Process(attackAccelerationTime);
        }

        public virtual void OnHitCallback(HitbackInfo hitback)
        {
            Debug.Log($"Попадание по {hitback.bodyPart}. Урон: {_customData.ProceedDamage(hitback.bodyPart == BodyPart.Head)}");
        }

        protected virtual void InputLogic()
        {
            if (_customData == null) return;
            if (SceneNetworkContext.Instance == null || SceneNetworkContext.Instance.GetStatistic(Statistics.STATISTIC_HEALTH)?.Value <= 0) return;
            bool attackPressed = (_attackOnClick && Input.GetMouseButtonDown(0)) || (!_attackOnClick && Input.GetMouseButton(0));
            if (_availableForAttack && attackPressed && _attackTimer >= _customData.attackSpeed)
            {
                StartCoroutine(ProceedAttack());
                _attackTimer = 0;
            }
            attackAccelerationTime = Mathf.Clamp01(attackAccelerationTime - Time.deltaTime * _recoilDeceleration);
        }

        private void SendHandFollow()
        {
            if (_availableForAttack && _weaponHandler != null)
            {
                //Ray rayShow = new Ray(_weaponHandler.CameraTransform.position, _weaponHandler.CameraTransform.forward);
                //if (Physics.Raycast(rayShow, out var hit, Mathf.Infinity))
                //    _weaponHandData.value[0] = hit.point;
                //else
                    _weaponHandData.value[0] = _weaponHandler.CameraTransform.position + _weaponHandler.CameraTransform.forward * 100000f;

                if (_followPointLeftHand != null)
                {
                    _weaponHandData.value[1] = _followPointLeftHand.position;
                    _weaponHandData.value[2] = _followPointLeftHand.eulerAngles;
                }

                Translator.Send(PlayerNetworkProtocol.SetWeaponAimer, _weaponHandData);
            }
        }

        protected void HitEffect(AttackData attackData)
        {
            Ray rayShot = new Ray(attackData.originPosition, attackData.direction);
            if (Physics.Raycast(rayShot, out var hit, 1000f, ~attackData.ignoreMask, QueryTriggerInteraction.Collide))
            {
                HandleHit(hit);
            }
        }

        protected virtual void HandleHit(RaycastHit hit) { }

        protected bool CheckAndUseAmmoPack(Services.Backend.Inventory.Item ammo, ItemSlot itemSlot, UISlot uiSlot)
        {
            InventoryPlus.Item tmpItem = itemSlot.GetItemType();
            if (tmpItem.itemID == ammo.ItemId)
            {
                var item = _catalogManager.GetItemData(tmpItem.itemID) as ConsumableItem;
                currentAmmo.Value += item.customData.countPerUnit;

                SceneNetworkContext.Instance.SaveStatistic();
                //var customData = ammo.CustomData;
                //if (customData.Count == 0)
                //    return false;
                //if ((ConsumableType)Int32.Parse(customData["consumableType"]) == ConsumableType.Ammo)
                //    SceneNetworkContext.Instance.RevokeItemFromUser(ammo.instanceIds.First(), _ => { });
                //else if ((ConsumableType)Int32.Parse(customData["consumableType"]) == ConsumableType.LaserAmmo)
                    SceneNetworkContext.Instance.SubtractUserVirtualCurrency(
                      $"SFT_{ammo.DisplayName}_" + ammo.ItemId, 
                      1, _ => { });

                ammo.count--;
                InventoryScreenManager.Instance.UseItem(uiSlot);
                return true;
            }
            return false;
        }

        protected ItemSlot CheckAmmo(Services.Backend.Inventory.Item[] ammos)
        {
            var inventory = InventoryScreenManager.Instance.Inventory;
            foreach (var ammo in ammos)
            {
                var uiSlot = inventory.GetUISlot(ammo.ItemId);
                if (uiSlot == null) continue;
                var itemSlot = inventory.GetItemSlot(ammo.ItemId);
                if (itemSlot == null) continue;

                if (CheckAndUseAmmoPack(ammo, itemSlot, uiSlot))
                    return itemSlot;
            }
            return null;
        }
    }
}