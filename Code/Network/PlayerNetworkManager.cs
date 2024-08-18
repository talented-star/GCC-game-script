using GrabCoin.UI.HUD;
using Mirror;
using System;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Zenject;
using UnityEngine.SceneManagement;
using PlayFab;
using System.Collections.Generic;
using GrabCoin.AsyncProcesses;
using GrabCoin.Config;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;

namespace GrabCoin.GameWorld.Network
{
    public class PlayerNetworkManager : NetworkManager
    {
        public event Action OnConnectedToServer;
        public event Action OnDisconnectedFromServer;
        public event Action<int> onPlayersNumberChanged;

        [Inject] private LoadingOverlay _loadingOverlay;
        [Inject] private PlayerScreensManager _screensManager;

        public bool IsConnected => NetworkClient.isConnected;

        private bool _clientStoped = true; // TODO: This variable seems unused. Use it in the logic or remove with OnStopClient() and OnStartClient() methods.
        private int _lastNumPlayers = -1;

        public static string FromLocationName = "";
        public static PlayerNetworkManager instance => (PlayerNetworkManager)singleton;
        public int PlayersNumber { get { return _lastNumPlayers; } }

#if UNITY_SERVER
        private SpawnPoint[] _spawnPoints;

        // TODO: Fix that
        [Inject]
        private void Construct()
        {
            _spawnPoints = FindObjectsOfType<SpawnPoint>();
        }
#endif
        private void RpcOnPlayersNumberChanged(int newCount)
        {
            onPlayersNumberChanged?.Invoke(newCount);
        }

        public void SetNetworkAddress(string networkAddress, ushort port)
        {
            Debug.Log($"Set network address:{networkAddress} and port:{port}");
            this.networkAddress = networkAddress;
            ((TelepathyTransport)transport).port = port;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            _clientStoped = true;
        } 
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("---------------------OnClientConnect");
            OnConnectedToServer?.Invoke();
            FromLocationName = "";
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }
        public async override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("---------------------OnClientDisconnect");
            OnDisconnectedFromServer?.Invoke();
            if (!string.IsNullOrWhiteSpace(FromLocationName))
            {
                var screen = await _screensManager.OpenPopup<InfoPopup>();
                screen.ProcessKey("The location is not available!");
                screen.onClose = async () =>
                {
                    instance.SetNetworkAddress(ScenePortConfig.GetIP(), ScenePortConfig.GetPort(FromLocationName));
                    await new LoadSceneWithLoadingTitle(FromLocationName, _loadingOverlay).Run();
                    FromLocationName = "";
                    instance.StartClient();
                };
            }
        }

        [Server]
        private void UpdatePlayerCount ()
        {
            if (numPlayers != _lastNumPlayers)
            {
                _lastNumPlayers = numPlayers;
                SceneNetworkContext.Count = _lastNumPlayers;
                //OnPlayersNumberChanged?.Invoke(_lastNumPlayers);
                //Debug.Log($"PlayerNetworkManager.UpdatePlayerCount(): Players Online: {PlayersNumber}");
            }
        }

#if UNITY_SERVER
        public override async void OnServerConnect(NetworkConnectionToClient connection)
        {
            Debug.Log("-----------------OnServerConnect");

            // TODO: We have already performed that in the Construct() method. Do we have dynamically changed set of spawn points?
            _spawnPoints = FindObjectsOfType<SpawnPoint>();

            Tuple<Vector3, float> spawnInfo = await ValidatePlayerSpawn();
            Vector3 startPosition = spawnInfo.Item1;
            float startRotation = spawnInfo.Item2;

            if (startPosition == null)
            {
                //TODO: handle connection error - we interrupting the connection establishing here.
                //TODO make reconnect to previous server and spawn or ban player
                return;
            }

            base.OnServerConnect(connection);
            //Debug.Log("<<<<<<<<<<<<<Request to spawn SceneNetworkContext and other>>>>>>>>>>>>>>");
            //NetworkServer.SpawnObjects();

            //if (SceneManager.GetActiveScene().name == "EmptyScene") return;
            CreatePlayer(connection, startPosition, startRotation);
            UpdatePlayerCount();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.Log("-----------------OnServerDisconnect");

            base.OnServerDisconnect(conn);
            UpdatePlayerCount();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);


            Debug.Log("<<<<<<<<<<<<<Request to spawn SceneNetworkContext and other>>>>>>>>>>>>>>");
            NetworkServer.SpawnObjects();
            UpdatePlayerCount();
        }

        private void CreatePlayer(NetworkConnectionToClient connection, Vector3 startPosition, float startRotation)
        {
            GameObject player = startPosition != null
                ? Instantiate(playerPrefab, startPosition, /* Quaternion.identity */ Quaternion.Euler(0, startRotation, 0))
                : Instantiate(playerPrefab);

            NetworkServer.AddPlayerForConnection(connection, player);

            //player.GetComponent<GrabCoin.GameWorld.Player.Player>().playerName = player.GetComponent<GrabCoin.GameWorld.Player.Player>().netId.ToString();
            UpdatePlayerCount();
        }

        //TODO add request for validation
        //recieve id of position or vector3 of position 
        private async Task<Tuple<Vector3, float>> ValidatePlayerSpawn()
        {
            // TODO: Remove hard-coded values
            //stub
            //await Task.Delay(2000);
            Vector3 _position = new Vector3(120, 60, 540);
            float _rotation = 0f;


            try
            {
                if (_spawnPoints.Length > 0)
                {
                    int positionId = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)].Id;
                    Transform tSelectedPoint = _spawnPoints
                                                .First(p => p.Id == positionId)
                                                .transform;
                    _position = tSelectedPoint.position;
                    _rotation = tSelectedPoint.eulerAngles.y;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return new Tuple<Vector3, float>(_position, _rotation);
        }
#else
    public override void LateUpdate ()
        {
            base.LateUpdate();
            //UpdatePlayerCount();
        }
#endif
        public override void OnDestroy()
        // private void OnDestroy()
        {
            PlayFabClientAPI.UpdateUserPublisherData(new PlayFab.ClientModels.UpdateUserDataRequest
            { Data = new Dictionary<string, string> { { "isOnline", "0" } } },
            result => { }, Debug.LogError );

            OnConnectedToServer = null;
            OnDisconnectedFromServer = null;
            onPlayersNumberChanged = null;
            base.OnDestroy();
        }
    }
}