using Mirror;
using UnityEngine;
using TMPro;
using Zenject;
using GrabCoin.Enum;
using Cysharp.Threading.Tasks;
using GrabCoin.Services.Chat;
using UnityEngine.XR.Management;
using System.Collections;
using GrabCoin.Model;
using UnityEngine.SceneManagement;
using System;
using NaughtyAttributes;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;
using GrabCoin.AsyncProcesses;
using GrabCoin.Config;
using GrabCoin.GameWorld.Network;
using System.Threading.Tasks;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.GameWorld.Weapons;
using PlayFab.ClientModels;
using PlayFab;
using PlayFabCatalog;
using InventoryPlus;
using static PlayFabCatalog.AddedCustomDataInPlayFabItems;
using GrabCoin.UI.HUD;
using System.Linq;
using GrabCoin.Services.Chat.VoiceChat;
using GrabCoin.Services.Backend.Inventory;
using Item = InventoryPlus.Item;
using Task = System.Threading.Tasks.Task;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
using EasyCharacterMovement;

namespace GrabCoin.GameWorld.Player
{
    public class Player : NetworkBehaviour
    {
        [Flags]
        public enum PlayerAnimation
        {
            None = 0,
            Crouch = 1 << 0,
            Jump = 1 << 1,
            Mining = 1 << 2,
            Aim = 1 << 3,
            Shoot = 1 << 4,
            Die = 1 << 5,
            Levitation = 1 << 6
        }

        [SerializeField] GameObject _thirdCharacterPrefab;
        [SerializeField] GameObject _vrCharacterPrefab;
        [SerializeField] GameObject _netCloneCharacterPrefab;
        [SerializeField] Transform _playerSpawnContext;
        [Space(20)]
        [SerializeField] ThirdPersonPlayerController _thirdPersonControl;
        [SerializeField] VRPersonController vrCharacter;
        [SerializeField] UnitMotor _thirdPersonCharacter;
        [SerializeField] AudioListener _audioListener;
        [SerializeField] bool _lockCursor;

        [SerializeField] private TextMeshProUGUI nickText;

        [SerializeField] private InventoryScreenManager inventoryScreen;

        [EnumFlags, SyncVar(hook = nameof(OnPlayerDieChanged))] private PlayerAnimation _playerAnimation = PlayerAnimation.None;
        [SyncVar] private Vector3 _move;
        [SyncVar] private Vector3 _direction;
        [SyncVar] private PlayerMode _mode = PlayerMode.None;
        [SyncVar] private bool _raidWithGC;
        [SyncVar(hook = nameof(OnPlayerNameChanged))] public string playerName;
        [SyncVar(hook = nameof(OnAvatarNameChanged))] public string _avatarName;
        [SyncVar(hook = nameof(OnEquipWeaponChanged))] private string _equipWaeponId;
        [SyncVar(hook = nameof(OnPlayerActiveShieldChanged))] private bool _isActiveShield;
        [SyncVar(hook = nameof(OnPlayerShieldChanged))] private float _shield = 0f;
        [SyncVar(hook = nameof(OnPlayerHealthChanged))] private float _health = 1000f;

        private List<Transform> _t2FollowOpenXR = new List<Transform>();

        private void OnPlayerHealthChanged(float _, float newHealth)
        {
            if (!isLocalPlayer) return;
            _health = newHealth;
            _healthStatistic.Value = (int)_health;
            Translator.Send(HUDProtocol.ChangedHealth, new FloatArrayData { value = new float[2] { newHealth, _customData.health } });
        }

        private void OnPlayerShieldChanged(float _, float newShield)
        {
            //Debug.Log($"New: {newShield}/{_} Local shield: {_shield}");
            //Debug.Log($"Is active shield: {_isActiveShield} Local shield: {_shield}");
            if (_isActiveShield && newShield < _)
                _energyShield.Flash();
            _shield = newShield;
            if (!isLocalPlayer) return;
            _energyStatistic.Value = (int)_shield;
            if (InventoryScreenManager.Instance.GetShield().isInit)
                Translator.Send(HUDProtocol.ChangedShield, new FloatArrayData { value = new float[2] { newShield, InventoryScreenManager.Instance.GetShieldCapacity() } });
        }

        private void OnPlayerActiveShieldChanged(bool _, bool newHealth)
        {
            Debug.Log($"Confirm shield state: {newHealth}");
            _isActiveShield = newHealth;
            if (!isLocalPlayer) return;
            Translator.Send(HUDProtocol.EnableShield, new BoolData { value = InventoryScreenManager.Instance.GetShield().isInit });
        }

        private Vector3 _startPosition;
        private Vector3 _lastPosition;

        [SyncVar] private Vector3 _vrTarget1p;
        [SyncVar] private Vector3 _vrTarget2p;
        [SyncVar] private Vector3 _vrTarget3p;
        [SyncVar] private Vector3 _vrTarget1r;
        [SyncVar] private Vector3 _vrTarget2r;
        [SyncVar] private Vector3 _vrTarget3r;

        private CustomEvent _onNetworkEvent;

        private StatisticValue _energyStatistic;
        private StatisticValue _healthStatistic;
        private StatisticValue _staminaStatistic;
        private CharacterCustomData _customData;
        private VRAvatarController _openXR;
        private NetCloneCharacter _follower;
        private EnergyShield _energyShield;

        private Factory<ThirdPersonPlayerController> _factoryCharacter;
        private Factory<VRPersonController> _factory;
        private Factory<NetCloneCharacter> _factoryNetClone;
        private PlayerState _playerState;
        private UserModel _userModel;
        private Controls _controls;
        private UIScreensManager _screensManager;
        private UIGameScreensManager _gameScreensManager;
        private LoadingOverlay _loadingOverlay;
        private CatalogManager _catalogManager;
        private InventoryDataManager _inventoryDataManager;
        private PlayerScreensManager _playerScreensManager;

        private float _healthCooldown;
        private float _batteryCooldown;
        private float _maxHealthCooldown;
        private float _maxBatteryCooldown;
        private float _stamina;
        private bool _isBlockedStamina;

        public AuthInfo AuthInfo => _userModel.AuthInfo;
        public float CostRun => _customData.costRun;
        public float CostJump => _customData.costJump;
        public float CostAbility => _customData.costAbility;
        public bool CanUseDash => _customData.characterType != CharacterType.Ilon;
        public float InventoryLimit
        {
            get
            {
                if (_customData == null)
                {
                    Debug.LogError("Player custom data is NULL");
                    return 846;
                }
                return _customData.maxVolumeInventory;
            }
        }

        [Inject]
        private void Construct(
            Factory<ThirdPersonPlayerController> factoryCharacter,
            Factory<VRPersonController> factory,
            Factory<NetCloneCharacter> factoryNetClone,
            PlayerState playerState,
            UserModel userModel,
            Controls controls,
            UIScreensManager screensManager,
            UIGameScreensManager gameScreensManager,
            LoadingOverlay loadingOverlay,
            CatalogManager catalogManager,
            InventoryDataManager inventoryDataManager,
            PlayerScreensManager playerScreensManager
            )
        {
            _factoryCharacter = factoryCharacter;
            _factory = factory;
            _factoryNetClone = factoryNetClone;
            _playerState = playerState;
            _userModel = userModel;
            _controls = controls;
            _screensManager = screensManager;
            _gameScreensManager = gameScreensManager;
            _loadingOverlay = loadingOverlay;
            _catalogManager = catalogManager;
            _inventoryDataManager = inventoryDataManager;
            _playerScreensManager = playerScreensManager;
        }

        private void Awake()
        {
            _energyShield = GetComponentInChildren<EnergyShield>();
            if (SceneManager.GetActiveScene().name is "StartupServer" or "Startup")
            {
                gameObject.SetActive(false);
                return;
            }
        }

        public async void Start()
        {
            //Debug.Log("Player.Start() START");
            if (SceneManager.GetActiveScene().name is "StartupServer" or "Startup")
            {
                gameObject.SetActive(false);
                //Debug.Log($"Player.Start() FINISH: scene \"{SceneManager.GetActiveScene().name}\"");
                return;
            }
            if (_catalogManager == null)
            {
              //Debug.Log("===>>> _catalogManager == null");
            }
            //Debug.Log("Player.Start() 1");
            await _catalogManager.WaitInitialize();
            //Debug.Log("Player.Start() 1.1");
            string characterKey = PlayerPrefs.GetString(SelectCharacterMenu.SELECTED_CHARACTER_KEY, "c_base");
            if (_catalogManager.GetItemData(characterKey) == null)
                await _catalogManager.CashingItem(characterKey);
            ItemData itemData = _catalogManager.GetItemData(characterKey);
            //Debug.Log("Player.Start() 1.2");
            if (itemData == null)
            {
                //Debug.Log("Player.Start() 1.3: itemData == null");
            }
            else
            {
                //Debug.Log("Player.Start() 1.4: itemData != null");
            }
            _customData = (itemData as CharacterItem).customData;
            //Debug.Log("Player.Start() 2");
            
            if (isLocalPlayer)
            {
                //Debug.Log("Player.Start() 3");
                var manager = new GameObject();
                //Debug.Log("Player.Start() 4");
                manager.AddComponent<ScreenOverlayManager>();
                //Debug.Log("Player.Start() 5");

                _t2FollowOpenXR.Clear();
                //Debug.Log("Player.Start() 6");
                if (_playerState.PlayerMode == PlayerMode.ThirdPerson)
                {
                    //Debug.Log("Player.Start() 7");
                    await Spawn3DCharacter();
                    //Debug.Log("Player.Start() 8");

                    _thirdPersonControl.SetPlayer(this);
                    //Debug.Log("Player.Start() 9");
                }
                else if (_playerState.PlayerMode == PlayerMode.VR)
                {
                    //Debug.Log("Player.Start() 10");
                    // StartCoroutine(EnableXR());
                    await SpawnVRCharacter();
                    //Debug.Log("Player.Start() 11");
                }
                //Debug.Log("Player.Start() 12");
                _onNetworkEvent = OnCustomPlayerNetworkEvent;
                //Debug.Log("Player.Start() 13");
                Translator.Add<PlayerNetworkProtocol>(_onNetworkEvent);
                //Debug.Log("Player.Start() 14");
                Translator.Add<UIPlayerProtocol>(_onNetworkEvent);
                //Debug.Log("Player.Start() 15");
                await CreateAndInitScreens();
                //Debug.Log("Player.Start() 16");

                switch (SceneManager.GetActiveScene().name)
                {
                    case "LocCity":
                        //Debug.Log("Player.Start() 17");
                        if (PlayerPrefs.GetInt("Visited Station", 0) == 0)
                        {
                            //Debug.Log("Player.Start() 18");
                            var screen = await _playerScreensManager.OpenScreen<InfoScreen>();
                            //Debug.Log("Player.Start() 19");
                            screen.Process("Welcome to Sky Harbor", "Welcome to Sky Harbor Info", "MinimapStation");
                            //Debug.Log("Player.Start() 20");
                            PlayerPrefs.SetInt("Visited Station", 1);
                            //Debug.Log("Player.Start() 21");
                        }
                        //Debug.Log("Player.Start() 22");
                        break;
                    case "LocJungles":
                        //Debug.Log("Player.Start() 23");
                        if (PlayerPrefs.GetInt("Visited Jungles", 0) == 0)
                        {
                            //Debug.Log("Player.Start() 24");
                            var screen = await _playerScreensManager.OpenScreen<InfoScreen>();
                            //Debug.Log("Player.Start() 25");
                            screen.Process("Welcome to Zone X", "Welcome to Zone X Info", "MinimapJungles");
                            //Debug.Log("Player.Start() 26");
                            PlayerPrefs.SetInt("Visited Jungles", 1);
                            //Debug.Log("Player.Start() 27");
                        }
                        //Debug.Log("Player.Start() 28");
                        break;
                }
                //Debug.Log("Player.Start() 29");


                PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
                    result =>
                    {
                        //Debug.Log("Player.Start() 34");
                        string name = result.AccountInfo.TitleInfo.DisplayName;
                        //Debug.Log("Player.Start() 35");
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            //Debug.Log("Player.Start() 36");
                            name = result.AccountInfo.Username;
                            //Debug.Log("Player.Start() 37");
                        }
                        //Debug.Log("Player.Start() 38");
                        CmdChangeName(name);
                        //Debug.Log("Player.Start() 39");
                    }, Debug.LogError);
                //Debug.Log("Player.Start() 40");
                _energyStatistic = SceneNetworkContext.Instance.GetStatistic(Statistics.STATISTIC_ENERGY);
                //Debug.Log("Player.Start() 41");
                CmdEnergyShield(_energyStatistic.Value);
                //Debug.Log("Player.Start() 42");
                //_energyShield.Init(_energyStatistic);
                _healthStatistic = SceneNetworkContext.Instance.GetStatistic(Statistics.STATISTIC_HEALTH);
                //Debug.Log("Player.Start() 43");
                if (_inventoryDataManager != null && _inventoryDataManager.IsFinishRaid)
                {
                    //Debug.Log("Player.Start() 30");
                    _inventoryDataManager.IsFinishRaid = false;
                    //Debug.Log("Player.Start() 31");
                    CmdInitHealth((int)_customData.health);
                    await _playerScreensManager.OpenPopup<FinishRaidScreen>();
                    //Debug.Log("Player.Start() 32");
                }
                //Debug.Log("Player.Start() 33");
                else
                {
                    CmdInitHealth(_healthStatistic.Value);
                    //Debug.Log("Player.Start() 44");
                }
                _staminaStatistic = SceneNetworkContext.Instance.GetStatistic(Statistics.STATISTIC_STAMINA);
                //Debug.Log("Player.Start() 45");
                _stamina = _staminaStatistic.Value;
                //Debug.Log("Player.Start() 46");

                CmdRaidWithGC(_playerState.raidWithGC);
                //Debug.Log("Player.Start() 47");

                Translator.Send(HUDProtocol.ChangedStamina, new FloatArrayData { value = new float[2] { _customData.stamina, _customData.stamina } });
                //Debug.Log("Player.Start() 48");

                await UniTask.Delay(1000);
                //Debug.Log("Player.Start() 49");
                var inventory = InventoryScreenManager.Instance.Inventory;
                //Debug.Log("Player.Start() 50");
                var itemIndex = inventory.GetItemIndex(inventory.hotbarUISlots[0]);
                //Debug.Log("Player.Start() 51");
                var tmpitem = inventory?.GetItemSlot(itemIndex)?.GetItemType();
                //Debug.Log("Player.Start() 52");
                await UniTask.WaitUntil(() => InventoryScreenManager.Instance.InputReader != null);
                //Debug.Log("Player.Start() 53");
                int currentAmmo = SceneNetworkContext.Instance.GetStatistic("currentAmmo").Value;
                //Debug.Log("Player.Start() 54");
                int ammoBox = InventoryScreenManager.Instance.Inventory.GetItemData("i_ammo_pack")?.GetItemNum() ?? 0; // todo: check have ammo
                //Debug.Log("Player.Start() 55");
                Translator.Send(HUDProtocol.CountBullet, new StringData { value = $"{currentAmmo}/{ammoBox}" });
                //Debug.Log("Player.Start() 56");
                InventoryScreenManager.Instance.InputReader.OnHotbarSlotSelected += OnHotbarSlotSelected;
                //Debug.Log("Player.Start() 57");
                string weaponId = "";
                //Debug.Log("Player.Start() 58");
                if (tmpitem != null &&
                    tmpitem.itemCategory == "weapon")
                {
                    //Debug.Log("Player.Start() 59");
                    weaponId = tmpitem.itemID;
                    //Debug.Log("Player.Start() 60");
                }
                //Debug.Log("Player.Start() 61");
                InventoryScreenManager.Instance.Inventory.OnSwap();
                //Debug.Log("Player.Start() 62");
                CmdEquipWeapon(weaponId);
                //Debug.Log("Player.Start() 63");
            }
            else if (!isServer)//(!string.IsNullOrEmpty(_avatarName))
            {
                CmdNameSelectedCharacter("");
                if (!_isActiveShield)
                    CmdCheckActiveShield();
            }
            //Debug.Log("Player.Start() 64");
            InitializeChat();
            //Debug.Log("Player.Start() 65");
            //Debug.Log("Player.Start() FINISH");
        }

        private async Task CreateAndInitScreens()
        {
            //Debug.Log("Player.CreateAndInitScreens() START");
            CreateInventoryScreen();
            //Debug.Log("Player.CreateAndInitScreens() 1/4");
            await _playerScreensManager.CreateScreen<BankScreen>();
            //Debug.Log("Player.CreateAndInitScreens() 2/4");
            await UniTask.NextFrame();
            //Debug.Log("Player.CreateAndInitScreens() 3/4");
            var screen = await _playerScreensManager.OpenScreen<GameHud>();
            //Debug.Log("Player.CreateAndInitScreens() 4/4");
            screen.SetFollowingHotbar(InventoryScreenManager.Instance.Inventory.hotbarUISlots);
            //Debug.Log("Player.CreateAndInitScreens() FINISH");
        }

        private void OnEnable()
        {
            if (isLocalPlayer)
            {
            }
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                Debug.LogWarning("IDisconnected");
                SceneNetworkContext.Instance.SaveStatistic();
                Translator.Remove<PlayerNetworkProtocol>(_onNetworkEvent);
                Translator.Remove<UIPlayerProtocol>(_onNetworkEvent);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ////Debug.Log($"<color=blue>Cursor visible: {Cursor.visible}</color>");
                var ism = InventoryScreenManager.Instance;
                if (ism != null)
                {
                    var ir = ism.InputReader;
                    if (ir != null)
                        ir.OnHotbarSlotSelected -= OnHotbarSlotSelected;
                }
                if (_mode == PlayerMode.VR)
                {
                    CanvasVRAdopter.Instance.RestoreCanvasesParents();
                }
            }
        }

        private async void OnHotbarSlotSelected(int index)
        {
            var shield = InventoryScreenManager.Instance.GetShield();
            if (_isActiveShield != shield.isInit)
            {
                //Debug.Log($"Change shield. Equip state: {shield.isInit}");
                CmdSetActiveShield(shield.isInit);
                if (shield.isInit)
                {
                    CmdEnergyShield(_energyStatistic.Value);
                    CmdInitShield(shield.Value.ItemId);
                }
            }
            
            GetItemFromHotSlots(index, out Inventory inventory, out Item tmpitem);

            if (tmpitem == null) return;
            if (tmpitem.itemCategory == "Consumable")
            {
                if (_catalogManager.GetItemData(tmpitem.itemID) == null)
                    await _catalogManager.CashingItem(tmpitem.itemID);

                var item = _catalogManager.GetItemData(tmpitem.itemID) as ConsumableItem;
                var instance = InventoryScreenManager.Instance.InventoryDataManager.GetItemData(tmpitem.itemID);
                switch (item.customData.consumableType)
                {
                    case ConsumableType.Health:
                        //check cooldown and substract item
                        if (_healthCooldown > 0) return;
                        _maxHealthCooldown = _healthCooldown = item.customData.cooldown;

                        CmdHealth(tmpitem.itemID);
                        break;
                    case ConsumableType.Battery:
                        //_batteryCooldown
                        if (_batteryCooldown > 0) return;
                        _maxBatteryCooldown = _batteryCooldown = item.customData.cooldown;

                        CmdInjectBattery();
                        break;
                }
                SceneNetworkContext.Instance.SubtractUserVirtualCurrency(
                  $"SFT_{item.DisplayName}_" + instance.ItemId, 
                  1, _ =>
                {
                    instance.count--;
                });
                InventoryScreenManager.Instance.UseItem(inventory.hotbarUISlots[index]);
                if (tmpitem.itemPrefab != null && tmpitem.itemPrefab.TryGetComponent(out IUsable usable))
                    usable.Use();
            }
        }

        private static void GetItemFromHotSlots(int index, out Inventory inventory, out Item tmpitem)
        {
            inventory = InventoryScreenManager.Instance.Inventory;
            var itemIndex = inventory.GetItemIndex(inventory.hotbarUISlots[index]);
            tmpitem = inventory?.GetItemSlot(itemIndex)?.GetItemType();
        }

        public Vector3 GetPosition() =>
            _playerSpawnContext.position;


        public override void OnStartClient()
        {
            base.OnStartClient();

        }

        private void OnCustomPlayerNetworkEvent(System.Enum code, ISendData data)
        {
            switch (code)
            {
                case PlayerNetworkProtocol.RaidWithGC:
                    BoolData boolData = (BoolData)data;
                    _playerState.raidWithGC = boolData.value;
                    break;
                case PlayerNetworkProtocol.Attack:
                    CmdAttack(NetworkClient.localPlayer.gameObject, (AttackData)data);
                    break;
                case PlayerNetworkProtocol.EquipWeapon:
                    CmdEquipWeapon(((StringData)data).value);
                    break;
                case PlayerNetworkProtocol.SetWeaponAimer:
                    if (isLocalPlayer)
                        _thirdPersonControl.SetWeaponHandPoints(((Vector3ArrayData)data));
                    break;
                case PlayerNetworkProtocol.SetWeaponFollower:

                    break;
                case UIPlayerProtocol.ChangeName:
                    if (!isLocalPlayer) return;
                    CmdChangeName(((StringData)data).value);
                    break;
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdChangeName(string newName)
        {
            playerName = newName;
        }

        [Command(requiresAuthority = false)]
        private void CmdEquipWeapon(string equipWeaponId)
        {
            _follower?.SpawnWeaponInHand(equipWeaponId);
            _equipWaeponId = equipWeaponId;
        }

        private void OnPlayerNameChanged(string _, string newName)
        {
            //Debug.Log($"Set player name:{_}->{newName}. Is Local character: {isLocalPlayer}");
            playerName = newName;
            if (nickText != null)
                nickText.text = newName;
            if (!isLocalPlayer)
            {
                _follower.SetName(playerName);
                CmdNameSelectedCharacter("");
            }
        }

        private void OnEquipWeaponChanged(string _, string newWeaponId)
        {
            if (isLocalPlayer) return;

            _follower?.SpawnWeaponInHand(newWeaponId);
        }

        private void OnMiningChanged(bool _, bool newValue)
        {
            if (isLocalPlayer) return;

            if (newValue != _ && _mode == PlayerMode.ThirdPerson)
            {
                if (newValue)
                    _follower?.StartMining();
                else
                    _follower?.StopMining();
            }
        }

        private void OnPlayerDieChanged(PlayerAnimation _, PlayerAnimation newValue)
        {
            _playerAnimation = newValue;
            if (!isLocalPlayer) return;
            if ((newValue & PlayerAnimation.Die) == PlayerAnimation.Die)
            {
                WaitRespawn();
            }
        }

        private void FixedUpdate()
        {
            if (_mode == PlayerMode.None) return;

            if (!isLocalPlayer)
            {
                switch (_mode)
                {
                    case PlayerMode.ThirdPerson:
                        _follower?.SetPosition(_playerSpawnContext.transform, _move, _direction, _playerAnimation);
                        break;
                    case PlayerMode.VR:
                        _follower?.SetVRPosition(_playerSpawnContext.transform.position,
                            _vrTarget1p,
                            _vrTarget2p,
                            _vrTarget3p
                            );
                        _follower?.SetVRRotation(
                            _vrTarget1r,
                            _vrTarget2r,
                            _vrTarget3r
                            );
                        break;
                }

            }
            else
            {
                var follow = _playerState.PlayerMode == PlayerMode.ThirdPerson ?
                    _thirdPersonControl.transform :
                    _openXR.transform;

                _playerSpawnContext.transform.position = follow.position;
                _playerSpawnContext.transform.rotation = follow.rotation;

                if (_mode == PlayerMode.VR)
                {
                    CmdSyncVRTargetsPosition(
                        _openXR.Head.vrTarget.position,
                        _openXR.LeftHand.vrTarget.position,
                        _openXR.RightHand.vrTarget.position);
                    CmdSyncVRTargetsRotation(
                        _openXR.Head.vrTarget.eulerAngles,
                        _openXR.LeftHand.vrTarget.eulerAngles,
                        _openXR.RightHand.vrTarget.eulerAngles);

                    foreach (var t in _t2FollowOpenXR)
                    {
                        Vector3 pos = t.position;
                        pos.x = _openXR.Head.vrTarget.position.x;
                        pos.z = _openXR.Head.vrTarget.position.z;
                        t.position = pos;
                        Vector3 rot = t.eulerAngles;
                        rot.y = _openXR.Head.vrTarget.eulerAngles.y;
                        t.rotation = Quaternion.Euler(rot);
                    }
                }
                else if (_mode == PlayerMode.ThirdPerson)
                {
                    var die = _thirdPersonCharacter.die;
                    _thirdPersonCharacter.SetDie((_playerAnimation & PlayerAnimation.Die) == PlayerAnimation.Die);
                    if (_thirdPersonCharacter.die && _thirdPersonCharacter.die != die)
                    {
                        _inventoryDataManager.DropItemBuffer();
                        _inventoryDataManager.GrantFromBuffer(_inventoryDataManager.GetCurrencyBuffer(), _inventoryDataManager.GetItemBuffer());
                        //Debug.Log("Character die");
                    }
                }

                CooldownUseHealth();
                CooldownUseBattery();
                RecoveryStamina();
            }
        }

        private async void CooldownUseHealth()
        {
            if (_healthCooldown >= 0)
            {
                _healthCooldown -= Time.fixedDeltaTime;
                for (int i = 0; i < InventoryScreenManager.Instance.Inventory.hotbarUISlots.Count; i++)
                {
                    GetItemFromHotSlots(i, out Inventory inventory, out Item tmpitem);
                    if (tmpitem != null && tmpitem.itemCategory == "Consumable")
                    {
                        if (_catalogManager.GetItemData(tmpitem.itemID) == null)
                            await _catalogManager.CashingItem(tmpitem.itemID);

                        var item = _catalogManager.GetItemData(tmpitem.itemID) as ConsumableItem;
                        if (item.customData.consumableType == ConsumableType.Health)
                            InventoryScreenManager.Instance.Inventory.hotbarUISlots[i].UpdateCooldown(_healthCooldown / _maxHealthCooldown);
                    }
                }
            }
        }

        private async void CooldownUseBattery()
        {
            if (_batteryCooldown >= 0)
            {
                _batteryCooldown -= Time.fixedDeltaTime;
                for (int i = 0; i < InventoryScreenManager.Instance.Inventory.hotbarUISlots.Count; i++)
                {
                    GetItemFromHotSlots(i, out Inventory inventory, out Item tmpitem);
                    if (tmpitem != null && tmpitem.itemCategory == "Consumable")
                    {
                        if (_catalogManager.GetItemData(tmpitem.itemID) == null)
                            await _catalogManager.CashingItem(tmpitem.itemID);

                        var item = _catalogManager.GetItemData(tmpitem.itemID) as ConsumableItem;
                        if (item.customData.consumableType == ConsumableType.Battery)
                            InventoryScreenManager.Instance.Inventory.hotbarUISlots[i].UpdateCooldown(_batteryCooldown / _maxBatteryCooldown);
                    }
                }
            }
        }

        private void RecoveryStamina()
        {
            if (_staminaStatistic == null) return;
            if (_stamina < _customData.stamina)
            {
                _stamina += _customData.rateStaminaRecovery * Time.fixedDeltaTime;
                _staminaStatistic.Value = (int)_stamina;
                if (_staminaStatistic.Value > _customData.stamina)
                    _stamina = _staminaStatistic.Value = (int)_customData.stamina;

                Translator.Send(HUDProtocol.ChangedStamina, new FloatArrayData { value = new float[2] { _stamina, _customData.stamina } });
            }
        }

        public bool CanGetStamina(float countStamina)
        {
            if (_isBlockedStamina) return false;

            if (_stamina >= countStamina)
            {
                _stamina -= countStamina;
                _staminaStatistic.Value = (int)_stamina;
                Translator.Send(HUDProtocol.ChangedStamina, new FloatArrayData { value = new float[2] { _stamina, _customData.stamina } });

                if (_stamina <= _customData.stamina * 0.01f)
                    StartCoroutine(CooldownBlockedStamina());

                return true;
            }
            return false;
        }

        private IEnumerator CooldownBlockedStamina()
        {
            _isBlockedStamina = true;
            yield return new WaitForSeconds(2);
            _isBlockedStamina = false;
        }

        public void DropOff(float power)
        {
            TargetDropOff(power);
        }

        [Command]
        private void CmdSyncVRTargetsPosition(Vector3 target1, Vector3 target2, Vector3 target3)
        {
            _vrTarget1p = target1;
            _vrTarget2p = target2;
            _vrTarget3p = target3;
        }

        [Command]
        private void CmdSyncVRTargetsRotation(Vector3 target1, Vector3 target2, Vector3 target3)
        {
            _vrTarget1r = target1;
            _vrTarget2r = target2;
            _vrTarget3r = target3;
        }

        [Command]
        public void CmdSetNetworkMoveVars(Vector3 currentPosition, Vector3 moveValue, Vector3 directionValue, PlayerAnimation playerAnimation)
        {
            _lastPosition = currentPosition;
            _move = moveValue;
            _direction = directionValue;
            if (_health <= 0)
                playerAnimation |= PlayerAnimation.Die;
            _playerAnimation = playerAnimation;
        }

        [Command]
        private void CmdSetPlayerMode(PlayerMode playerMode)
        {
            _mode = playerMode;
        }

        [Command(requiresAuthority = false)]
        public void CmdAttack(GameObject netIdentity, AttackData attackData)
        {
            _follower.Shoot(netIdentity, AttackCallback, attackData);
            TargetAttack(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, attackData);
        }

        [TargetRpc]
        public void TargetAttack(NetworkConnectionToClient _, AttackData attackData)
        {
            _follower?.Shoot(_.identity.gameObject, AttackCallback, attackData);
        }

        [Command(requiresAuthority = false)]
        public void CmdRaidWithGC(bool data)
        {
            _raidWithGC = data;
        }

        private void AttackCallback(GameObject netIdentity, HitbackInfo info)
        {
            TargetAttackCallback(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, info.Serialize()/*info.bodyPart, info.isLastHit, info.classHitTarget, info.grantValue*/);
        }
#if UNITY_EDITOR
        [TargetRpc]
        public void TargetDropOff(float power)
        {
            if (_mode == PlayerMode.ThirdPerson)
            {
                _thirdPersonControl.UnitMotor.characterMovement.PauseGroundConstraint();
                _thirdPersonControl.UnitMotor.characterMovement.LaunchCharacter(Vector3.up * power, true);
            }
        }
#else
        [TargetRpc]
        public void TargetDropOff(float power)
        {
            if (_mode == PlayerMode.ThirdPerson)
            {
                _thirdPersonControl.UnitMotor.characterMovement.PauseGroundConstraint();
                _thirdPersonControl.UnitMotor.characterMovement.LaunchCharacter(Vector3.up * power, true);
            }
        }
#endif
        [TargetRpc]
        private void TargetAttackCallback(NetworkConnectionToClient _, byte[] data/*BodyPart bodyPart, bool isLastHit, ClassHitTarget classHitTarget, int grantValue*/)
        {
            //TODO: add experience
            if (!isLocalPlayer) return;

            HitbackInfo info = HitbackInfo.Deserialize(data);
            if (info.isLastHit)
            {
                if (info.classHitTarget == ClassHitTarget.Player && info.grantValue > 0)
                {
                    _inventoryDataManager.AddCurrencyToBuffer("SC", info.grantValue);
                    //Debug.Log($"Grant {info.grantValue * 0.01f:F2} GC for lastHit");
                }
            }
            Translator.Send(HUDProtocol.SetHealthBarValue, new HudBarData { value = info.remainingHealth});
            Translator.Send(HUDProtocol.CreateUIEffect, new DamageEffectData { isCrit = info.bodyPart == BodyPart.Head, damage = ((int)info.damage).ToString() });
        }

        [TargetRpc]
        private void TargetRespawn()
        {
            if (!isLocalPlayer) return;
            var follow = _playerState.PlayerMode == PlayerMode.ThirdPerson ?
                    _thirdPersonControl.transform :
                    _openXR.transform;
            //Debug.Log("RESPAWN");
            follow.localPosition = Vector3.zero;
        }

        //TODO for setting active when transfer between scenes
        internal void SetActive(bool active)
        {

        }

        private async Task<bool> Spawn3DCharacter()
        {
            string characterName = await CheckSelectCharacter();

            EventSystemsController.Instance.SetEventSystemForFlatscreen();
            if (_catalogManager.GetItemData(characterName) == null)
                await _catalogManager.CashingItem(characterName);

            var data = _catalogManager.GetItemData(characterName) as CharacterItem;
            GameObject prefab = data.itemConfig.Prefab.GetComponent<AvatarContainer>().avatar3d;
            _thirdPersonControl = _factoryCharacter.Create(prefab);
            _thirdPersonControl.transform.SetParent(transform);
            _thirdPersonControl.transform.localPosition = Vector3.zero;
            _thirdPersonControl.transform.localRotation = Quaternion.identity;

            _thirdPersonCharacter = _thirdPersonControl.GetComponent<UnitMotor>();
            _thirdPersonCharacter.maxSpeed = _customData.walkSpeed;
            _thirdPersonCharacter.runSpeedMultiplier = _customData.walkSpeed;
            _thirdPersonCharacter.isDashActivity = _customData.characterType == CharacterType.Tiger;
            _thirdPersonCharacter.isLevitateActivity = _customData.characterType == CharacterType.Dragon;

            _audioListener = _thirdPersonControl.GetComponentInChildren<AudioListener>();
            _audioListener.enabled = isClient && isLocalPlayer;

            GetComponent<VoicePresenter>().SetCharacter(_thirdPersonControl);

            int childCount = _playerSpawnContext.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = _playerSpawnContext.GetChild(0);
                child.SetParent(_thirdPersonControl.transform);
                // child.localPosition = new Vector3(0, child.position.y, 0);
                child.localPosition = new Vector3(0, child.localPosition.y, 0);
            }

            //Cursor.lockState = _lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            //Cursor.visible = !_lockCursor;

            CmdSetPlayerMode(PlayerMode.ThirdPerson);
            CmdNameSelectedCharacter(characterName);
            return true;
        }

        private string[] _OpenXRChildren = { "NickName" };
        private bool IsOpenXRChild (Transform t)
        {
            string n = t.name;
            foreach (var s in _OpenXRChildren) if (n.Equals(s)) return true; 
            return false;
        }
        private async Task<bool> SpawnVRCharacter()
        {
            string characterName = await CheckSelectCharacter();

            EventSystemsController.Instance.SetEventSystemForVR();
            if (_catalogManager.GetItemData(characterName) == null)
                await _catalogManager.CashingItem(characterName);

            var data = _catalogManager.GetItemData(characterName) as CharacterItem;
            GameObject prefab = data.itemConfig.Prefab.GetComponent<AvatarContainer>().avatarVr;

            string prefabName = "NewVrAvatar"; // "YANA/Prefabs/NewVrAvatar";
            GameObject newVrAvatarPrefab = (GameObject)UnityEngine.Resources.Load(prefabName, typeof(GameObject));

            // var vrCharacter = _factory.Create(_vrCharacterPrefab);
            vrCharacter = _factory.Create(newVrAvatarPrefab);
            vrCharacter.transform.SetParent(transform);
            vrCharacter.transform.localPosition = Vector3.zero;
            vrCharacter.transform.localRotation = Quaternion.identity;
            VRPlayerController vrPlayerController = vrCharacter.GetComponent<VRPlayerController>();
            vrPlayerController.SetPlayer(this, true);
            vrPlayerController.SetAvatar(prefab);
            _openXR = vrCharacter.GetComponentInChildren<VRAvatarController>();
            //if (_openXR == null) //Debug.Log("===>>>---->>> Player.SpawnVRCharacter: _openXR == null");

            _audioListener = vrCharacter.GetComponentInChildren<AudioListener>();
            _audioListener.enabled = isClient && isLocalPlayer;

            CmdSetPlayerMode(PlayerMode.VR);
            CmdNameSelectedCharacter(characterName);

            int childCount = _playerSpawnContext.childCount;
            for (int i = 0; i < childCount; i++)
                _playerSpawnContext.GetChild(0).SetParent(vrCharacter.transform);
            for (int i = 0; i < vrCharacter.transform.childCount; i++)
            {
                Transform t = vrCharacter.transform.GetChild(i);
                if (IsOpenXRChild(t))
                {
                    _t2FollowOpenXR.Add(t);
                }
            }
            Transform tHead = _openXR.Head.vrTarget;
            Camera camera = tHead.GetComponent<Camera>();
            CanvasVRAdopter.Instance?.AdoptUI2VR(camera, tHead);
            return true;
        }

        private async UniTask SpawnNetCloneCharacter(string name)
        {
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") START");
            while (true)
            {
                if (_mode != PlayerMode.None)
                    break;
                await UniTask.DelayFrame(1);
            }
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 1");
            if (string.IsNullOrEmpty(name))
            {
                //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 1.1");
                string bufferName = _avatarName;
                _avatarName = "";
                await Task.Delay(50);
                _avatarName = bufferName;
            }
            else
            {
                //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 1.2");
                _avatarName = name;
            }
            //_avatarName = string.IsNullOrEmpty(name) ? _avatarName : name;
            if (_follower != null)
                return;
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 2");
            var data = _catalogManager.GetItemData(name) as CharacterItem;
            if (data == null)
            {
                await _catalogManager.CashingItem(name);
                data = _catalogManager.GetItemData(name) as CharacterItem;
            }
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 3");
            if (data == null) {} //Debug.Log("===>>> ERROR: data == null"); }
            else
            {
                if (data.itemConfig == null) { } //Debug.Log("===>>> ERROR: data.itemConfig == null"); }
                else
                {
                    if (data.itemConfig.Prefab == null) { } //Debug.Log("===>>> ERROR: data.itemConfig.Prefab == null"); }
                }
            }
            GameObject prefab = data.itemConfig.Prefab.GetComponent<AvatarContainer>().avatarNet;
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 4");
            _follower = _factoryNetClone.Create(prefab);
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 5");
            _follower.transform.parent.SetParent(transform);
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 6");
            _follower.transform.parent.localPosition = Vector3.zero;
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 7");
            _follower.transform.parent.localRotation = Quaternion.identity;
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") 8");

            _follower.InitializeMode(_catalogManager, _mode, DamageControl);

            //int childCount = _playerSpawnContext.childCount;
            //for (int i = 0; i < childCount; i++)
            //{
            //    Transform child = _playerSpawnContext.GetChild(0);
            //    child.SetParent(_follower.transform);
            //    child.localPosition = new Vector3(0, child.position.y, 0);
            //}
            //Debug.Log($"Player.SpawnNetCloneCharacter(\"{name}\") FINISH");
        }

        private void OnAvatarNameChanged(string _, string newName)
        {
            if (!isLocalPlayer && !string.IsNullOrEmpty(newName))
            {
                //Debug.Log($"Get avatar name: \"{newName}\"");
                SpawnNetCloneCharacter(newName).Forget();
            }
        }

        private UniTask<string> CheckSelectCharacter()
        {
            var completion = new UniTaskCompletionSource<string>();
            string selectCharacter = "c_base";
#if !UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(_userModel.AuthInfo.SignSignature))
            {
                //Debug.Log("METAMASK authorize!");
#endif
                SceneNetworkContext.Instance.GetUserPublisherData("selectCharacter", result =>
                {
                    if (result.isInit)
                    {
                        var data = result.Value.Data;
                        if (data.ContainsKey("selectCharacter"))
                        {
                            string name = data["selectCharacter"].Value;
                            if (!string.IsNullOrWhiteSpace(name))
                                selectCharacter = name;
                        }
                    }
                    completion.TrySetResult(selectCharacter);
                });
#if !UNITY_EDITOR
            }
            else
                completion.TrySetResult(selectCharacter);
#endif
            return completion.Task;
        }

        private void CreateInventoryScreen()
        {
            //Debug.Log("Player.CreateInventoryScreen() START");
            if (InventoryScreenManager.Instance)
            {
                //Debug.Log("Player.CreateInventoryScreen() 1/16");
                InventoryScreenManager.Instance.Init(InventoryLimit);
                //Debug.Log("Player.CreateInventoryScreen() ABORT");
                return;
            }
            //Debug.Log("Player.CreateInventoryScreen() 2/16");
            var targetCanvasGO = new GameObject("InventoryCanvas");
            //Debug.Log("Player.CreateInventoryScreen() 3/16");
            targetCanvasGO.transform.parent = ProjectContext.Instance.transform;
            //Debug.Log("Player.CreateInventoryScreen() 4/16");

            var targetCanvas = targetCanvasGO.AddComponent<Canvas>();
            //Debug.Log("Player.CreateInventoryScreen() 5/16");
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            //Debug.Log("Player.CreateInventoryScreen() 6/16");

            var targetCanvasScaler = targetCanvasGO.AddComponent<CanvasScaler>();
            //Debug.Log("Player.CreateInventoryScreen() 7/16");
            targetCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            //Debug.Log("Player.CreateInventoryScreen() 8/16");
            targetCanvasScaler.referenceResolution = new Vector2(1920, 1080);
            //Debug.Log("Player.CreateInventoryScreen() 9/16");
            targetCanvasScaler.matchWidthOrHeight = 1f;
            //Debug.Log("Player.CreateInventoryScreen() 10/16");

            targetCanvasGO.AddComponent<GraphicRaycaster>();
            //Debug.Log("Player.CreateInventoryScreen() 11/16");

            var screen = Instantiate(inventoryScreen, targetCanvasGO.transform);
            //Debug.Log("Player.CreateInventoryScreen() 12/16");
            screen.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            //Debug.Log("Player.CreateInventoryScreen() 13/16");
            screen.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            //Debug.Log("Player.CreateInventoryScreen() 14/16");

            screen.Init(InventoryLimit);
            //Debug.Log("Player.CreateInventoryScreen() 15/16");

            _playerScreensManager.RegisterScreen(screen);
            //Debug.Log("Player.CreateInventoryScreen() 16/16");
            screen.Close();
            //Debug.Log("Player.CreateInventoryScreen() FINISH");
        }

        private void DamageControl(HitInfo info, BodyPart part) //on NetClone on server side
        {
            if (isLocalPlayer && _thirdPersonControl.IsSafeble) return;
            if (_health <= 0) return;
            float damage = info.customData.ProceedDamage(part == BodyPart.Head);
            float logDamage = damage;
            string log = $"Damage: {logDamage} ";
            if (_isActiveShield)
            {
                _energyShield.EnergyControl(ref damage);
                _shield = _energyShield.CurrentCapacity;
                log += $"Shield: {logDamage - damage}/{_energyShield.CurrentCapacity} ";
            }
            else
                log += "No Shield ";
            _health -= damage;
            log += $"Health: {damage}/{_health}";
            //Debug.Log(log);
            HitbackInfo hitbackInfo = new HitbackInfo(part, damage, _health / _customData.health);
            hitbackInfo.classHitTarget = ClassHitTarget.Player;
            hitbackInfo.isLastHit = false;
            if (_health <= 0)
            {
                hitbackInfo.isLastHit = true;
                if (_raidWithGC)
                    hitbackInfo.grantValue = (int)(500 * 0.4f);
                Respawn();
            }
            info.shooterCallback?.Invoke(info.netIdentity, hitbackInfo);
            _follower?.SetLifeState(_health <= 0);
        }

        [Command]
        private void CmdHealth(string healthId)
        {
            CheckAndUseHealth(healthId);
        }

        private async void CheckAndUseHealth(string healthId)
        {
            if (_catalogManager.GetItemData(healthId) == null)
                await _catalogManager.CashingItem(healthId);

            var data = (_catalogManager.GetItemData(healthId) as ConsumableItem).customData;
            Health(data);
        }

        [Command(requiresAuthority = false)]
        private void CmdInjectBattery()
        {
            _energyShield.InjectBattery();
        }

        [Command(requiresAuthority = false)]
        private void CmdInitHealth(int health)
        {
            _health = health;
        }

        [Command(requiresAuthority = false)]
        private void CmdSetActiveShield(bool isActive)
        {
            _isActiveShield = isActive;
            //Debug.Log($"Change state shield: {_isActiveShield}");
        }

        [Command(requiresAuthority = false)]
        private void CmdCheckActiveShield()
        {
            CheckShield();
        }

        private async void CheckShield()
        {
            bool tmp = _isActiveShield;
            _isActiveShield = !_isActiveShield;
            await Task.Delay(10);
            _isActiveShield = tmp;
        }

        [Command(requiresAuthority = false)]
        private void CmdEnergyShield(float energy)
        {
            _shield = energy;
            //Debug.Log($"Change capacity shield: {_shield}");
        }

        [Command(requiresAuthority = false)]
        private void CmdInitShield(string id)
        {
            CheckAndEquipShield(id);
        }

        private async void CheckAndEquipShield(string id)
        {
            if (_catalogManager.GetItemData(id) == null)
                await _catalogManager.CashingItem(id);

            var shield = _catalogManager.GetItemData(id) as ShieldItem;
            if (_energyShield != null)
                _energyShield.onRegenTick = null;
            _energyShield.Init(shield.customData, _shield);
            _energyShield.onRegenTick = Recharge;
        }

        [Command(requiresAuthority = false)]
        private void CmdNameSelectedCharacter(string name)
        {
            //Debug.Log($"CmdNameSelectedCharacter: {name}");
            SpawnNetCloneCharacter(name).Forget();
        }

        [Command]
        private void Respawn()
        {
            TargetRespawn();
            _health = _customData.health;
            _follower?.SetLifeState(_health <= 0);
        }

        private void Recharge(float energy)
        {
            _shield = energy;
        }

        private async void Health(ConsumableCustomData customData)
        {
            int tick = 0;
            while (tick < customData.actionTick)
            {
                var health = _health + (customData.countPerUnit / customData.actionTick);
                _health = health < _customData.health ? health : _customData.health;
                await Task.Delay(1000);
                tick++;
            }
        }

        private async void WaitRespawn()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
            await UniTask.Delay(3000);
            var screen = await _playerScreensManager.OpenScreen<RespawnScreen>();
            var result = await screen.Process();
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
            _healthStatistic.Value = (int)_customData.health;
            Respawn();
            TransferScene();
        }

        private async void TransferScene(bool result = true)
        {
            if (!result) return;
            SetActive(false); //TODO for setting active when transfer between scenes

            ValidateTransferResponseModel response = await ValidateTransferRequest();

            if (response == null) //TODO returning player to some world position or respawn
                SetActive(true);
            else
            {
                PlayerNetworkManager.instance.StopClient();
                PlayerNetworkManager.instance.SetNetworkAddress(response.NetworkAddress, response.Port);
                await new LoadSceneWithLoadingTitle("LocCity", _loadingOverlay).Run();
                PlayerNetworkManager.instance.StartClient();
            }
        }

        private async Task<ValidateTransferResponseModel> ValidateTransferRequest()
        {
            //TODO send request with point id for transfer validation
            //if ok, get ip, scene name and try spawn

            await Task.Delay(0); //stub validation delay

            return new ValidateTransferResponseModel(/* "185.225.34.121" */ ScenePortConfig.GetIP(), ScenePortConfig.GetPort("LocCity"), "LocCity");
        }

        private IEnumerator EnableXR()
        {
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
            }
            else
            {
                //Debug.Log("Starting XR...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                yield return SpawnVRCharacter();
            }
        }

        private void InitializeChat()
        {
            var chatPresenter = GetComponent<ChatPresenter>();
            chatPresenter.Initialize(this, GetComponent<ChatNetwork>());
        }

        public Camera GetViewCamera()
        {
            switch (_mode)
            {
                case PlayerMode.VR:
                    return vrCharacter.GetComponentInChildren<Camera>();
                case PlayerMode.ThirdPerson:
                    return _thirdPersonControl.GetCameraController().GetComponentInChildren<Camera>();
                default:
                    return null;
            }
        }

        public Transform GetPlayer()
        {
            switch (_mode)
            {
                case PlayerMode.VR:
                    return vrCharacter.transform;
                case PlayerMode.ThirdPerson:
                    return _thirdPersonCharacter.transform;
                default:
                    return null;
            }
        }

        public UnitMotor GetUnitMotor()
        {
            return _thirdPersonCharacter;
        }

        public PlayerMode GetMode()
        {
            return _mode;
        }
    }
}