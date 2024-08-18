using GrabCoin.AsyncProcesses;
using GrabCoin.Config;
using GrabCoin.GameWorld.Player;
using GrabCoin.Loader;
using GrabCoin.Services;
using GrabCoin.Services.Backend.Inventory;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.SearchService;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GrabCoin.GameWorld.Network
{
    public class TransferPoint : MonoBehaviour
    {

        public enum RaidDirection { None, StartRaid, FinishRaid }
        [SerializeField] private Transform _returnPoint;
        //TODO remove after server validation added
        //[SerializeField] private string _transferNetworkAddress;
        [SerializeField] private RaidDirection _raidDirection;
        // [SerializeField] private ushort _transferPort;
//#if UNITY_EDITOR
//        [ListToPopup(typeof(TransferPoint), "_scenes")]
//#endif
        [SerializeField, NaughtyAttributes.Scene] private string _transferScene;

        [SerializeField] private TMP_Text _nameLocation;
        [SerializeField] private TMP_Text _nameLocationBackSide;
        [SerializeField] private bool _isLockedForVR = false;
        public static string[] _scenes;
        private PlayerNetworkManager _playerNetworkManager;
        private LoadingOverlay _loadingOverlay;
        private UIPopupsManager _popupsManager;
        private InventoryDataManager _inventoryManager;
        private PlayerScreensManager _screensManager;

//#if UNITY_EDITOR
//        private void Reset()
//        {
//            _transferScene = SceneManager.GetSceneAt(0).name;
//            _scenes = SceneExt.GetScenes();
//        }

//        [ContextMenu("Update Scenes")]
//        private void OnValidate()
//        {
//            _scenes = SceneExt.GetScenes();
//        }
//#endif

        [Inject]
        private void Construct(
            PlayerNetworkManager playerNetworkManager,
            LoadingOverlay loadingOverlay,
            UIPopupsManager popupsManager,
            InventoryDataManager inventoryManager,
            PlayerScreensManager screensManager
            )
        {
            _playerNetworkManager = playerNetworkManager;
            this._loadingOverlay = loadingOverlay;
            _popupsManager = popupsManager;
            _inventoryManager = inventoryManager;
            _screensManager = screensManager;
            _nameLocation.text = "";
            _nameLocationBackSide.text = "";
            string[] res = Regex.Split(_transferScene, "(?=\\p{Lu})");
            for (int i = 2; i < res.Length; i++)
            {
                _nameLocation.text += res[i] + " ";
                _nameLocationBackSide.text += res[i] + " ";
            }
        }

#if !UNITY_SERVER
        private async void OnTriggerEnter(Collider other)
        {
            bool isVrCollider = "XR Origin".Equals(other.transform.parent?.parent?.name);
            if ((other.transform.parent.gameObject == NetworkClient.localPlayer?.gameObject) || isVrCollider)
            {
                if (isVrCollider && _isLockedForVR)
                {
                    XR_UI.Instance.ShowScreen(XR_UI.Screen.LocationLockedForVr);
                }
                else if (_raidDirection == RaidDirection.StartRaid)
                {
                    if (!_screensManager.EqualsCurrentScreen<GameHud>() && !isVrCollider) return;
                    var screen = await _screensManager.OpenPopup<StartRaidScreen>();
                    var result = await screen.Process();

                    if (result)
                        TransferScene();
                }
                else if (_raidDirection == RaidDirection.FinishRaid)
                {
                    _inventoryManager.UnblockedPayRaid();
                    _inventoryManager.IsFinishRaid = true;
                    TransferScene();
                }
                else
                    TransferScene();
            }
        }

        private async void TransferScene(bool result = true)
        {
            if (!result) return;
            Player.Player player = NetworkClient.localPlayer.GetComponent<Player.Player>();

            player.SetActive(false); //TODO for setting active when transfer between scenes

            ValidateTransferResponseModel response = await ValidateTransferRequest();

            if (response == null) //TODO returning player to some world position or respawn
                player.SetActive(true);
            else
            {
                PlayerNetworkManager.instance.StopClient();
                PlayerNetworkManager.instance.SetNetworkAddress(response.NetworkAddress, response.Port);
                PlayerNetworkManager.FromLocationName = SceneManager.GetActiveScene().name;
                await new LoadSceneWithLoadingTitle(_transferScene, _loadingOverlay).Run();
                PlayerNetworkManager.instance.StartClient();
            }
        }

        private async Task<ValidateTransferResponseModel> ValidateTransferRequest()
        {
            //TODO send request with point id for transfer validation
            //if ok, get ip, scene name and try spawn
            
            await Task.Delay(0); //stub validation delay

            return new ValidateTransferResponseModel(ScenePortConfig.GetIP(), ScenePortConfig.GetPort(_transferScene), _transferScene);
        }
#endif
                }
            }