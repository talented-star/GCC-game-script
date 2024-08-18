using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.UI.HUD;
using InventoryPlus;
using PlayFab;
using PlayFabCatalog;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Weapons
{
    public class WeaponHandler : MonoBehaviour, IWeaponHandler
    {
        [Header("Properties")]
        [SerializeField] protected string _itemCategory = "weapon";
        [Header("References")]
        [SerializeField] protected Transform _rightHand;
        [SerializeField] protected WeaponBase _weaponSystem;
        [SerializeField] protected UnitMotor _unitMotor;
        [SerializeField] protected ThirdPersonCameraController _cameraController;
        [SerializeField] protected Animator _animator;
        [SerializeField] private WeaponAimer _weaponAimer;

        private CatalogManager _catalogManager;
        private RuntimeAnimatorController _defaultController;
        private Item selectedItem;

        public WeaponBase WeaponBase
        {
            get { return _weaponSystem; }
        }

        public Transform CameraTransform => _cameraController.transform;

        [Inject]
        private void Construct(CatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        public Vector3 TransformPoint(Vector3 attackPointOffset)
        {
            return transform.TransformPoint(attackPointOffset);
        }

        private async void OnEnable()
        {
            await UniTask.WaitUntil(() => InventoryScreenManager.Instance != null);
            await UniTask.WaitUntil(() => InventoryScreenManager.Instance.InputReader != null);
            Debug.LogWarning("IConnected");
            if (InventoryScreenManager.Instance)
                InventoryScreenManager.Instance.InputReader.OnHotbarSlotSelected += OnHotbarSlotSelected;
        }

        private void OnDisable()
        {
            Debug.LogWarning("IDisconnected");
            if (InventoryScreenManager.Instance)
                InventoryScreenManager.Instance.InputReader.OnHotbarSlotSelected -= OnHotbarSlotSelected;
        }

        public void OnHotbarSlotSelected(int index)
        {
            var inventory = InventoryScreenManager.Instance.Inventory;
            var itemIndex = inventory.GetItemIndex(inventory.hotbarUISlots[index]);
            var tmpitem = inventory?.GetItemSlot(itemIndex)?.GetItemType();

            if (tmpitem != null && tmpitem.itemCategory != _itemCategory) return;

            AimGUIType lastType = AimGUIType.None;
            if (_weaponSystem != null)
            {
                lastType = _weaponSystem.AimGUIType;
                Destroy(_weaponSystem.gameObject);
                if (_animator)
                    _animator.runtimeAnimatorController = _defaultController;
                selectedItem = null;
                Translator.Send(PlayerNetworkProtocol.EquipWeapon, new StringData { value = "" });
            }

            if (tmpitem?.itemCategory == _itemCategory && (selectedItem == null || tmpitem != selectedItem)) 
            {
                var weaponData = (_catalogManager.GetItemData(tmpitem.itemID) as EquipmentItem).customData;
                if (tmpitem.itemUpgradeLevel > 0)
                {
                    var upgradeId = _catalogManager.GetBuyUpgradeItems()[tmpitem.itemUpgradeLevel - 1].ItemId;
                    var upgradeData = (_catalogManager.GetItemData(upgradeId) as UpgradeItem).customData;
                    weaponData.damage += weaponData.damage * upgradeData.damage;
                }

                var weaponPrefab = Instantiate(tmpitem.itemPrefab, _rightHand);
                weaponPrefab.transform.localPosition = Vector3.zero;
                weaponPrefab.transform.localRotation = Quaternion.identity;
                _weaponSystem = weaponPrefab.GetComponent<WeaponBase>();

                _weaponSystem.SetAvailableForAttack(false);
                _weaponSystem.Initialize(this, weaponData);
                _weaponAimer?.Initialize(this);

                _weaponSystem.Initialize(this, weaponData);
                _weaponAimer?.Initialize(this);
                _weaponSystem.SetAvailableForAttack(true);

                bool isAiming = _unitMotor?.isAiming ?? true;

                if(lastType != _weaponSystem.AimGUIType)
                {
                    switch (lastType)
                    {
                        case AimGUIType.AimCross:
                            Translator.Send(HUDProtocol.AimCross, new BoolData { value = false });
                            break;
                        case AimGUIType.Scope:
                            Translator.Send(HUDProtocol.AimScope, new BoolData { value = false });
                            break;
                    }
                }

                switch (_weaponSystem.AimGUIType)
                {
                    case AimGUIType.AimCross:
                        Translator.Send(HUDProtocol.AimCross, new BoolData { value = isAiming });
                        break;
                    case AimGUIType.Scope:
                        Translator.Send(HUDProtocol.AimScope, new BoolData { value = isAiming });
                        break;
                }

                if (_animator != null && (_weaponSystem?.AnimatorOverrideController))
                    _animator.runtimeAnimatorController = _weaponSystem?.AnimatorOverrideController;
                selectedItem = tmpitem;

                Translator.Send(PlayerNetworkProtocol.EquipWeapon, new StringData { value = selectedItem.itemID });
            }
            else
            {
                _cameraController?.SetDefaultFOV();
                switch (lastType)
                {
                    case AimGUIType.AimCross:
                        Translator.Send(HUDProtocol.AimCross, new BoolData { value = false });
                        break;
                    case AimGUIType.Scope:
                        Translator.Send(HUDProtocol.AimScope, new BoolData { value = false });
                        break;
                }
            }
        }

        private async void Start()
        {
            _defaultController = _animator.runtimeAnimatorController;
            if (_weaponSystem != null)
            {
                var inventory = InventoryScreenManager.Instance.Inventory;
                var itemIndex = inventory.GetItemIndex(inventory.hotbarUISlots[0]);
                var tmpitem = inventory?.GetItemSlot(itemIndex)?.GetItemType();
                var weaponData = (_catalogManager.GetItemData(tmpitem.itemID) as EquipmentItem).customData;

                _weaponSystem.Initialize(this, weaponData);
                _weaponSystem.SetAvailableForAttack(false);
            }

            _weaponAimer?.Initialize(this);

            await UniTask.WaitUntil(() => InventoryScreenManager.Instance != null);
            await UniTask.WaitUntil(() => InventoryScreenManager.Instance.InputReader != null);

            InventoryScreenManager.Instance.InputReader.SelectCurrentHotbarSlot();
        }

        private void Update()
        {
            if (!_weaponSystem)
                return;

            bool aiming = _unitMotor?.isAiming ?? true;

            if (aiming != _weaponSystem.GetAvailableForAttack())
                Translator.Send(_weaponSystem.AimGUIType == AimGUIType.AimCross ? HUDProtocol.AimCross : HUDProtocol.AimScope, new BoolData { value = aiming });
            
            _weaponSystem.SetAvailableForAttack(aiming);
            _cameraController.SetFOVAmount(aiming ? _weaponSystem.AimingZoomAmount : 1f);
            _animator.SetBool("HasWeapon", _weaponSystem != null);
        }
    }
}