using TMPro; // TODO: Work with HUD not directly changing the text field
using Mirror;
using UnityEngine;
using System.Linq;
using System;
using PlayFab;
using GrabCoin.UI.ScreenManager;
using GrabCoin.Services.Backend.Inventory;
using Zenject;
using GrabCoin.UI.Screens;
using System.Collections.Generic;
using GrabCoin.Services.Backend.Catalog;
using Cysharp.Threading.Tasks;
using PlayFab.ClientModels;
using UnityEngine.TextCore.Text;
using StatisticValue = PlayFab.ClientModels.StatisticValue;
using GetPlayerStatisticsRequest = PlayFab.ClientModels.GetPlayerStatisticsRequest;
using UpdatePlayerStatisticsRequest = PlayFab.ClientModels.UpdatePlayerStatisticsRequest;
using StatisticUpdate = PlayFab.ClientModels.StatisticUpdate;
using PlayFabCatalog;
using Code.Services.AuthService;
using Newtonsoft.Json;
using GrabCoin.AsyncProcesses;
using UpdateUserDataRequest = PlayFab.ClientModels.UpdateUserDataRequest;
using GetUserDataResult = PlayFab.ClientModels.GetUserDataResult;
using GetUserDataRequest = PlayFab.ClientModels.GetUserDataRequest;
// using UnityEditor.PackageManager;
#if ENABLE_PLAYFABSERVER_API
using SubtractUserVirtualCurrencyRequest = PlayFab.ServerModels.SubtractUserVirtualCurrencyRequest;
using AddUserVirtualCurrencyRequest = PlayFab.ServerModels.AddUserVirtualCurrencyRequest;
using PlayFab.ServerModels;
#endif

namespace GrabCoin.UI.HUD
{
    [Serializable]
    public class CurrencyBalance
    {
        public decimal GC;
    }

    public partial class SceneNetworkContext : NetworkBehaviour
    {
        //private Optionals<CurrencyBalance> _serverBalance;

        public static SceneNetworkContext Instance { get; private set; }

        //private Action<bool> _answerSuccessTransfer;
        //private Action<bool> _successSellItems;
        private Dictionary<Guid, Action<bool>> _answerCallback = new();

        [SyncVar(hook = nameof(OnChangePlayerCount))] [SerializeField] private int _playerCounter;
        private TextMeshProUGUI _playerCountText;

        private UIPopupsManager _popupsManager;
        private InventoryDataManager _inventoryDataManager;
        private CatalogManager _catalogManager;
        private PlayerScreensManager _screensManager;
        private EmailAuthService _emailAuthService;

        private CustomSignal _customSignal;

        public static int Count
        {
            set
            {
                if (Instance == null)
                    return;

                Instance._playerCounter = value;
            }
        }

        [Inject]
        private void Construct(
            UIPopupsManager popupsManager,
            InventoryDataManager inventoryDataManager,
            CatalogManager catalogManager,
            PlayerScreensManager screensManager,
            EmailAuthService emailAuthService
            )
        {
            _popupsManager = popupsManager;
            _inventoryDataManager = inventoryDataManager;
            _catalogManager = catalogManager;
            _screensManager = screensManager;
            _emailAuthService = emailAuthService;
        }

        private void Awake()
        {
            Instance = this;
#if !UNITY_SERVER
            _customSignal = OnRequestPlayerCount;
            Translator.Add<HUDProtocol>(_customSignal);
            CmdRequestPlayerCount();
#endif
        }

        private void OnDestroy()
        {
            Translator.Remove<HUDProtocol>(_customSignal);
        }

#region "Items"
        public void GrantItemToUser(string catalogId, Action<bool> callback)
        {
            var guid = Guid.NewGuid();
            _answerCallback.Add(guid, callback);
            CmdGrantItemToUser(NetworkClient.localPlayer.gameObject, PlayFabSettings.staticPlayer.PlayFabId, catalogId, guid);
        }

        public void RevokeItemFromUser(string itemInstanceId, Action<bool> callback)
        {
            var guid = Guid.NewGuid();
            _answerCallback.Add(guid, callback);
            CmdRevokeItemFromUser(NetworkClient.localPlayer.gameObject, PlayFabSettings.staticPlayer.PlayFabId, itemInstanceId, guid);
        }

        public void UpdateItemInstanceToUser(string itemInstanceId, string key, string value, Action<bool> callback)
        {
            var guid = Guid.NewGuid();
            _answerCallback.Add(guid, callback);
            CmdUpdateItemInstanceToUser(NetworkClient.localPlayer.gameObject, PlayFabSettings.staticPlayer.PlayFabId, itemInstanceId, key, value, guid);
        }

        [Command(requiresAuthority = false)]
        private void CmdGrantItemToUser(GameObject netIdentity, string playfabId, string catalogId, Guid answerId)
        {
#if ENABLE_PLAYFABSERVER_API
            PlayFabServerAPI.GrantItemsToUser(new GrantItemsToUserRequest
            {
                CatalogVersion = _catalogManager.GetItemData(catalogId).catalogItem.Value.CatalogVersion,
                ItemIds = new List<string>(new string[] { catalogId }),
                PlayFabId = playfabId
            }, result =>
            {
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, true, (Guid)result.CustomData);
            }, error =>
            {
                Debug.LogError(error);
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, (Guid)error.CustomData);
            }, answerId);
#endif
        }

        [Command(requiresAuthority = false)]
        private void CmdRevokeItemFromUser(GameObject netIdentity, string playfabId, string itemInstanceId, Guid answerId)
        {
#if ENABLE_PLAYFABSERVER_API
            PlayFabServerAPI.RevokeInventoryItem(new RevokeInventoryItemRequest
            {
                ItemInstanceId = itemInstanceId,
                PlayFabId = playfabId
            }, result =>
            {
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, true, (Guid)result.CustomData);
            }, error =>
            {
                Debug.LogError(error);
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, (Guid)error.CustomData);
            }, answerId);
#endif
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateItemInstanceToUser(GameObject netIdentity, string playfabId, string itemInstanceId, string key, string value, Guid answerId)
        {
#if ENABLE_PLAYFABSERVER_API
            PlayFabServerAPI.UpdateUserInventoryItemCustomData(new UpdateUserInventoryItemDataRequest
            {
                ItemInstanceId = itemInstanceId,
                PlayFabId = playfabId,
                Data = new Dictionary<string, string>
                {
                    { key, value }
                }
            }, result =>
            {
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, true, (Guid)result.CustomData);
            }, error =>
            {
                Debug.LogError(error);
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, (Guid)error.CustomData);
            }, answerId);
#endif
        }
#endregion "Items"

#region "Currency"
        public void AddUserVirtualCurrency(string nameToken, decimal countCurrency, Action<bool> callback)
        {
            APIConnect.SetTokenBalanceCall(new SetTokenBalanceData(PlayFabSettings.staticPlayer.PlayFabId, nameToken, countCurrency),
                result =>
                {
                    callback?.Invoke(true);
                },
                error =>
                {
                    callback?.Invoke(false);
                    Debug.LogError(error);
                });
        }

        public void SubtractUserVirtualCurrency(string nameToken, decimal countCurrency, Action<bool> callback)
        {
            APIConnect.SetTokenBalanceCall(new SetTokenBalanceData(PlayFabSettings.staticPlayer.PlayFabId, nameToken, -countCurrency),
                result =>
                {
                    callback?.Invoke(true);
                },
                error =>
                {
                    callback?.Invoke(false);
                    Debug.LogError(error);
                });
        }

        public void ConvertationUserVirtualCurrency(string nameCurrency, uint countCurrency, Action<bool> callback)
        {
            var guid = Guid.NewGuid();
            _answerCallback.Add(guid, callback);
            CmdConvertationUserVirtualCurrency(NetworkClient.localPlayer.gameObject, PlayFabSettings.staticPlayer.PlayFabId, nameCurrency, countCurrency, guid);
        }

        [Command(requiresAuthority = false)]
        private void CmdConvertationUserVirtualCurrency(GameObject netIdentity, string playfabId, string nameCurrency, uint countCurrency, Guid answerId)
        {
#if ENABLE_PLAYFABSERVER_API
            PlayFabServerAPI.SubtractUserVirtualCurrency(new SubtractUserVirtualCurrencyRequest
            {
                PlayFabId = playfabId,
                VirtualCurrency = nameCurrency,
                Amount = (int)countCurrency
            }, result =>
            {
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, true, (Guid)result.CustomData);
            }, error =>
            {
                Debug.LogError(error);
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, (Guid)error.CustomData);
            }, answerId);
#endif
        }

        public void SubtractUserCurrency(string nameCurrency, uint countCurrency, Action<bool> callback)
        {
            var guid = Guid.NewGuid();
            _answerCallback.Add(guid, callback);
            CmdConvertationUserVirtualCurrency(NetworkClient.localPlayer.gameObject, PlayFabSettings.staticPlayer.PlayFabId, nameCurrency, countCurrency, guid);
        }

        [Command(requiresAuthority = false)]
        private void CmdSubtractUserCurrency(GameObject netIdentity, string playfabId, string nameCurrency, uint countCurrency, Guid answerId)
        {
#if ENABLE_PLAYFABSERVER_API
            PlayFabServerAPI.SubtractUserVirtualCurrency(new SubtractUserVirtualCurrencyRequest
            {
                PlayFabId = playfabId,
                VirtualCurrency = nameCurrency,
                Amount = (int)countCurrency
            }, result =>
            {
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, true, (Guid)result.CustomData);
            }, error =>
            {
                Debug.LogError(error);
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, (Guid)error.CustomData);
            }, answerId);
#endif
        }

        #region "Old methods currency"
        public void AddUserVirtualCurrency(decimal curentCurrency, decimal countCurrency, Action<bool> callback)
        {
            var guid = Guid.NewGuid();
            _answerCallback.Add(guid, callback);
            CmdAddUserVirtualCurrency(NetworkClient.localPlayer.gameObject, PlayFabSettings.staticPlayer.PlayFabId, curentCurrency, countCurrency, guid);
        }

        [Command(requiresAuthority = false)]
        private void CmdAddUserVirtualCurrency(GameObject netIdentity, string playfabId, decimal curentCurrency, decimal countCurrency, Guid answerId)
        {
#if ENABLE_PLAYFABSERVER_API
            CheckServerAuth(result =>
            {
                if (!result)
                {
                    Debug.LogError("Server don`t authorization!");
                    OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, answerId);
                    return;
                }

                //if (!_serverBalance.isInit)
                //    GetServerBalance(() =>
                //    {
                //        TradeCurrencyFromServerToUser(netIdentity, playfabId, curentCurrency, countCurrency, answerId);
                //    }, answerId);
                //else
                //    TradeCurrencyFromServerToUser(netIdentity, playfabId, curentCurrency, countCurrency, answerId);
            });
#endif
        }

#if ENABLE_PLAYFABSERVER_API
        private void TradeCurrencyFromServerToUser(GameObject netIdentity, string playfabId, decimal curentCurrency, decimal countCurrency, Guid answerId)
        {
            //if (countCurrency > _serverBalance.Value.GC)
            //{
            //    countCurrency = _serverBalance.Value.GC;
            //    Debug.LogError("Server balance GC is empty!");
            //}

            //_serverBalance.Value.GC -= countCurrency;
            UpdateServerBalance();
            UpdateUserBalance(netIdentity, playfabId, curentCurrency, countCurrency, answerId);
        }

        private void UpdateServerBalance()
        {
            //string json = JsonConvert.SerializeObject(_serverBalance.Value);
            //UpdateUserPublisherData("currency balanse", json, result => { });
        }

        private void GetServerBalance(Action callback, Guid answerId)
        {
            var completion = new UniTaskCompletionSource();
            GetUserPublisherData("currency balanse", result =>
            {
                if (result.isInit)
                {
                    if (result.Value.Data.ContainsKey("currency balanse"))
                    {
                        //_serverBalance = new Optionals<CurrencyBalance>(JsonConvert.DeserializeObject<CurrencyBalance>(result.Value.Data["currency balanse"].Value));
                    }
                    else
                    {
                        Debug.LogError("Server haven`t currency balanse!");
                        OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, answerId);
                    }
                }
                else
                {
                    Debug.LogError("Server haven`t currency balanse!");
                    OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, answerId);
                }
                callback?.Invoke();
            });
        }

        private async void CheckServerAuth(Action<bool> callback)
        {
            if (!_emailAuthService.ValidateAuth().Result)
            {
                var result = await _emailAuthService.SignIn();
                callback?.Invoke(result);
            }
            else
                callback?.Invoke(true);
        }
#endif

        public void SubtractUserVirtualCurrency(decimal curentCurrency, decimal countCurrency, Action<bool> callback)
        {
            var guid = Guid.NewGuid();
            _answerCallback.Add(guid, callback);
            CmdSubtractUserVirtualCurrency(NetworkClient.localPlayer.gameObject, PlayFabSettings.staticPlayer.PlayFabId, curentCurrency, countCurrency, guid);
        }

        [Command(requiresAuthority = false)]
        private void CmdSubtractUserVirtualCurrency(GameObject netIdentity, string playfabId, decimal curentCurrency, decimal countCurrency, Guid answerId)
        {
#if ENABLE_PLAYFABSERVER_API
            CheckServerAuth(result =>
            {
                if (!result)
                {
                    Debug.LogError("Server don`t authorization!");
                    OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, answerId);
                    return;
                }

                //if (!_serverBalance.isInit)
                //    GetServerBalance(() =>
                //    {
                //        TradeCurrencyFromUserToServer(netIdentity, playfabId, curentCurrency, countCurrency, answerId);
                //    }, answerId);
                //else
                //    TradeCurrencyFromUserToServer(netIdentity, playfabId, curentCurrency, countCurrency, answerId);
            });
#endif
        }

#if ENABLE_PLAYFABSERVER_API
        private void TradeCurrencyFromUserToServer(GameObject netIdentity, string playfabId, decimal curentCurrency, decimal countCurrency, Guid answerId)
        {
            //_serverBalance.Value.GC += countCurrency;
            UpdateServerBalance();
            UpdateUserBalance(netIdentity, playfabId, curentCurrency, -countCurrency, answerId);
        }

        private void UpdateUserBalance(GameObject netIdentity, string playfabId, decimal curentCurrency, decimal countCurrency, Guid answerId)
        {
            string json = JsonConvert.SerializeObject(new CurrencyBalance { GC = curentCurrency + countCurrency });
            PlayFabServerAPI.UpdateUserPublisherData(new PlayFab.ServerModels.UpdateUserDataRequest
            {
                PlayFabId = playfabId,
                Data = new Dictionary<string, string>
                {
                    { "currency balanse", json }
                }
            }, result =>
            {
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, true, (Guid)result.CustomData);
            }, error =>
            {
                Debug.LogError(error);
                OnCallbackResult(netIdentity.GetComponent<NetworkIdentity>().connectionToClient, false, (Guid)error.CustomData);
            }, answerId);
        }
#endif
        #endregion "Old methods currency"
        #endregion "Currency"

        #region "PublisherData"
        public void UpdateUserPublisherData(string keyData, string dataJson, Action<bool> callback)
        {
            PlayFabClientAPI.UpdateUserPublisherData(new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { keyData, dataJson }
                }
            }, result =>
            {
                callback?.Invoke(true);
            }, error =>
            {
                Debug.LogError(error);
                callback?.Invoke(false);
            });
        }

        public void GetUserPublisherData(string keyData, Action<Optionals<GetUserDataResult>> callback)
        {
            PlayFabClientAPI.GetUserPublisherData(new GetUserDataRequest
            {
                Keys = new List<string> { keyData }
            }, result =>
            {
                callback?.Invoke(new Optionals<GetUserDataResult>(result));
            }, error =>
            {
                Debug.LogError(error);
                callback?.Invoke(default);
            });
        }

        public void GetAllUserBalances(Action<Optionals<GetUserDataResult>> callback)
        {
            PlayFabClientAPI.GetUserPublisherData(new GetUserDataRequest(), 
            result =>
            {
                callback?.Invoke(new Optionals<GetUserDataResult>(result));
            }, error =>
            {
                Debug.LogError(error);
                callback?.Invoke(default);
            });
        }
#endregion "PublisherData"

#region "PlayerCount"
        [Command(requiresAuthority = false)]
        private void CmdRequestPlayerCount()
        {
            Instance._playerCounter = NetworkServer.connections.Count(kv => kv.Value.identity != null);
        }

        private void OnChangePlayerCount(int _, int newCount)
        {
            Translator.Send(HUDProtocol.ChangePlayerCount, new IntData { value = newCount });
        }

        private void OnRequestPlayerCount(System.Enum code)
        {
            switch (code)
            {
                case HUDProtocol.RequestPlayerCount:
                    Translator.Send(HUDProtocol.ChangePlayerCount, new IntData { value = _playerCounter });
                    break;
            }
        }

#endregion "PlayerCount"

#region "Statistics"
        private static List<StatisticValue> _statisticValues = new();
        [SerializeField] private float _timer = 60;

#if !UNITY_SERVER
        private void Update()
        {
            if (_timer <= 0)
            {
                _timer = 120;
                if (PlayFabSettings.staticPlayer.IsClientLoggedIn())
                    SaveStatistic();
            }
            else
                _timer -= Time.deltaTime;
        }
#endif

        public StatisticValue GetStatistic(string nameStatistic)
        {
            return _statisticValues.Where(value => value.StatisticName == nameStatistic).FirstOrDefault();
        }

        public void CallGetStatistics()
        {
            PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest
            {
                StatisticNames = new List<string>
                {
                    Statistics.STATISTIC_HEALTH,
                    Statistics.STATISTIC_STAMINA,
                    Statistics.STATISTIC_AMMO,
                    Statistics.STATISTIC_LASER_AMMO,
                    Statistics.STATISTIC_ENERGY
                }
            },
            result =>
            {
                _statisticValues = result.Statistics;
                CheckStatistics();
            }, Debug.LogError);
        }

        public void SaveStatistic()
        {
#if !UNITY_SERVER
            PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
            {
                Statistics = _statisticValues.Select(value => new StatisticUpdate
                {
                    StatisticName = value.StatisticName,
                    Value = value.Value,
                    Version = value.Version
                }).ToList()
            }, result => { }, Debug.LogError);
#endif
        }

        private void CheckStatistics()
        {
            //await UniTask.WaitWhile(() => _catalogManager == null);
            //await _catalogManager.WaitInitialize();

            //var customData = (_catalogManager.GetItemData("с_base") as CharacterItem).customData;
            if (!_statisticValues.Any(value => value.StatisticName == Statistics.STATISTIC_HEALTH))
                _statisticValues.Add(new StatisticValue { StatisticName = Statistics.STATISTIC_HEALTH, Value = 3000, Version = 0 });
            if (!_statisticValues.Any(value => value.StatisticName == Statistics.STATISTIC_STAMINA))
                _statisticValues.Add(new StatisticValue { StatisticName = Statistics.STATISTIC_STAMINA, Value = 200, Version = 0 });
            if (!_statisticValues.Any(value => value.StatisticName == Statistics.STATISTIC_AMMO))
                _statisticValues.Add(new StatisticValue { StatisticName = Statistics.STATISTIC_AMMO, Value = 0, Version = 0 });
            if (!_statisticValues.Any(value => value.StatisticName == Statistics.STATISTIC_LASER_AMMO))
                _statisticValues.Add(new StatisticValue { StatisticName = Statistics.STATISTIC_LASER_AMMO, Value = 0, Version = 0 });
            if (!_statisticValues.Any(value => value.StatisticName == Statistics.STATISTIC_ENERGY))
                _statisticValues.Add(new StatisticValue { StatisticName = Statistics.STATISTIC_ENERGY, Value = 1000, Version = 0 });
            SaveStatistic();
        }
#endregion "Statistics"

        [TargetRpc]
        private void OnCallbackResult(NetworkConnectionToClient _, bool result, Guid answerId)
        {
            Debug.Log($"Get answer with guid: {answerId}");
            _answerCallback[answerId]?.Invoke(result);
            _answerCallback.Remove(answerId);
        }
    }
}
