using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using GrabCoin.Model;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using Mirror;
using Org.BouncyCastle.Bcpg.OpenPgp;
using PlayFab;
using PlayFabCatalog;
using Sources;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.GameWorld.Resources
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class MiningResource : NetworkBehaviour,
        IInteractable
#if UNITY_SERVER
        , INetworkAnswer
#endif
    {
        [SerializeField] private Image _progressIndicator;
        [SerializeField] private QuickOutline.Outline _outline;

        private Action<bool, IInteractable> _answerStartUsing;
        private Action<bool, IInteractable> _answerFinishUsing;
        private Action<GameObject> _onMined;

        [SyncVar] private string _id;
        [SyncVar] private bool _isInteracting;
        [SyncVar(hook = nameof(UpdateProgressView))] private float _progress;
        [SyncVar(hook = nameof(UpdateId))] private string _immunitetId = "";

        private const string _multitoolId = "eq_multitool";
        private EquipmentCustomData _multitoolData;
        private GameObject _ownedConnection;
        private float _immunitetTimer;
        private float _durability;

        private ResourceStats _stats;
        private CatalogManager _catalog;
        private InventoryDataManager _inventory;

        public bool IsCanInteract => true;

        public string Name => gameObject.name;

        public string ID => _id;

        [Inject]
        private void Construct(
            CatalogManager catalog,
            InventoryDataManager inventoryDataManager
            )
        {
            _catalog = catalog;
            _inventory = inventoryDataManager;
            _progressIndicator.transform.parent.gameObject.SetActive(false);
        }

        internal void Initialize(string resourceID, ResourceStats resourceStats, Action<GameObject> onMined)
        {
            _id = resourceID;
            _stats = resourceStats;
            _onMined = onMined;
            _multitoolData = (_catalog.GetItemData(_multitoolId) as EquipmentItem).customData;
            _durability = _stats.miningTimerInSec;
        }

        private void FixedUpdate()
        {
            if (isServer && !string.IsNullOrEmpty(_immunitetId))
            {
                _immunitetTimer -= Time.fixedDeltaTime;
                if (_immunitetTimer <= 0)
                    _immunitetId = "";
            }
        }

        public void Use(GameObject netIdentity, AuthInfo authInfo, Action<bool, IInteractable> answerStartUsing, Action<bool, IInteractable> answerFinishUsing)
        {
            _answerStartUsing = answerStartUsing;
            _answerFinishUsing = answerFinishUsing;
            if (_multitoolData == null)
                _multitoolData = (_catalog.GetItemData(_multitoolId) as EquipmentItem).customData;
            CmdUse(netIdentity, _multitoolData?.damage ?? 0.25f, authInfo.SignAccount);
            //Translator.Send(HUDProtocol.CreateUIEffect, new StringData { value = (_multitoolData?.damage ?? 0.25f).ToString("F2") });
        }

        [Command(requiresAuthority = false)]
        private void CmdUse(GameObject netIdentity, float damage, string userId)
        {
#if UNITY_SERVER
            if (_durability > 0 && (string.IsNullOrEmpty(_immunitetId) || _immunitetId.Equals(userId)))
            {
                Debug.Log("Get command to use resource");
                Debug.Log($"Player with id: {userId}");
                _immunitetTimer = 1f;
                _immunitetId = userId;
                _ownedConnection = netIdentity;

                _durability -= damage;
                AsyncUse();
            }

            //if (IsCanInteract)
            //{
            //    _isInteracting = true;
            //    StartImmunitetTimer();
            //}
            //else
            //{
            //    Debug.Log("Don`t can use");
            //    FailStartUsing(_ownedConnection.GetComponent<NetworkIdentity>().connectionToClient);
            //}
#endif
        }
#if UNITY_SERVER
        [Server]
        private async void AsyncUse()
        {
            await UniTask.Delay(20);
            if (_durability <= 0)
                _durability = 0;
            _progress = (_stats.miningTimerInSec - _durability) / _stats.miningTimerInSec;
            if (_durability <= 0)
                Success();
        }

        [Server]
        private async void StartImmunitetTimer()
        {
            await UniTask.Delay(1000);

            Debug.Log("Start use");
            //SuccessStartUsing(_ownedConnection.GetComponent<NetworkIdentity>().connectionToClient);
            _progress = 0;
            float timer = 0;
            while (_progress < 1f)
            {
                await UniTask.WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
                _progress = timer / _stats.miningTimerInSec;
            }
            Success();
        }

        [Server]
        public void Success()
        {
            Debug.Log("Added resource");
            SuccessFinishUsing(_ownedConnection.GetComponent<NetworkIdentity>().connectionToClient);
            _onMined?.Invoke(gameObject);
        }

        [Server]
        public void Failure()
        {
            throw new NotImplementedException();
        }
#endif

        [TargetRpc]
        private void SuccessStartUsing(NetworkConnectionToClient target)
        {
            Debug.Log("Start use");
            //_progressIndicator.gameObject.SetActive(true);
            _answerStartUsing?.Invoke(true, this);
        }

        [TargetRpc]
        private void FailStartUsing(NetworkConnectionToClient target)
        {
            Debug.Log("Failure start use resource");
            _answerStartUsing?.Invoke(false, this);
        }

        [TargetRpc]
        private void SuccessFinishUsing(NetworkConnectionToClient target)
        {
            Debug.Log("Success finish using");
            //_progressIndicator.gameObject.SetActive(false);
            _answerFinishUsing?.Invoke(true, this);
            _inventory.AddItemToBuffer(_id);
        }

        [TargetRpc]
        private void FailFinishUsing(NetworkConnectionToClient target)
        {
            //_progressIndicator.gameObject.SetActive(false);
            _answerFinishUsing?.Invoke(false, this);
        }

        [Client]
        public string GetName()
        {
            return string.IsNullOrEmpty(_id) ? "Resource witout name" : _catalog.GetItemData(_id).DisplayName;
        }

        private void UpdateId(string _, string newId)
        {
            _immunitetId = newId;
            Debug.Log($"Update immunitet id: {newId}");
        }

        private void UpdateProgressView(float _, float newProgress)
        {
            //_progressIndicator.fillAmount = newProgress;
            AudioManager.Instance.PlaySound3D("MinigOre", transform.position);
            if (PlayFabSettings.staticPlayer.PlayFabId == _immunitetId)
                Translator.Send(HUDProtocol.ShowHealthBarWithTime, new HudBarData { target = transform, value = 1f - newProgress, name = GetName() });
            //if (!_progressIndicator.transform.parent.gameObject.activeSelf)
            //    _progressIndicator.transform.parent.gameObject.SetActive(true);
        }

        public void Hightlight(bool isActive)
        {
            _outline.enabled = isActive;
        }

        public float GetWeight()
        {
            return _catalog.GetResourceData(_id).GetWeight();
        }
    }
}
