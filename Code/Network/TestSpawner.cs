using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.AsyncProcesses;
using GrabCoin.Config;
using GrabCoin.Loader;
using GrabCoin.Services;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.UI.ScreenManager;
using Mirror;
using PlayFab;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GrabCoin.GameWorld.Network
{
    public class TestSpawner : MonoBehaviour
    {
        [Inject] private LoadingOverlay _loadingOverlay;
        [Inject] private CatalogManager _catalogManager;
        [Inject] private EmailAuthService _emailAuthService;

        private int _delayBetweenAuthChecks = 20000;

        private async void Start()
        {
#if UNITY_SERVER
            DontDestroyOnLoad(gameObject);
            string email = "info@grabcoinclub.com";
            int hash = email.GetHashCode();
            if (hash < 0)
                hash *= -1;
            _emailAuthService.FillingData(hash.ToString(), email, "cfgkhjesru645vcbnhbjtr");
            var result = await _emailAuthService.SignIn();
            CheckAuth();
            if (!result)
                Debug.Log("Fail auth");
            else
                _catalogManager.Initialize();
            Debug.Log("-----------------ServerModeInit----------------------");
            PlayerNetworkManager.instance.SetNetworkAddress(ScenePortConfig.GetIP(), ScenePortConfig.GetPort(ScenePortConfig.GetLoadingScene()));
            PlayerNetworkManager.instance.StartServer();
            await new LoadScene(ScenePortConfig.GetLoadingScene()).Run();
#else
            //APIConnect.SetTokenBalanceCall(new SetTokenBalanceData("8FF15FC994A89C99", "GC", 10.05m),
            //            result =>
            //            {
            //                Debug.Log("TEST REQUEST SET BALANCE SUCCESS");
            //            }, Debug.LogError);
            Debug.Log("-----------------ClientModeInit----------------------");
            PlayerNetworkManager.instance.StopClient();
            PlayerNetworkManager.instance.SetNetworkAddress(ScenePortConfig.IsReleaseVersion ? ScenePortConfig.GetIP() : "185.225.34.121", ScenePortConfig.GetPort("Startup"));
            await new LoadSceneWithLoadingTitle("Startup", _loadingOverlay).Run();
            PlayerNetworkManager.instance.StartClient();
#endif
        }


#if UNITY_SERVER
        private async void CheckAuth()
        {
            await UniTask.Delay(_delayBetweenAuthChecks /* 60000 */);
            bool isStaticPlayerNull = (PlayFabSettings.staticPlayer == null);
            bool isClientLoggedIn = isStaticPlayerNull ? false : PlayFabSettings.staticPlayer.IsClientLoggedIn();
            if (isStaticPlayerNull || !isClientLoggedIn)
            {
                Debug.Log("!!!AUTH!!! Try auth...");
                string email = "info@grabcoinclub.com";
                int hash = email.GetHashCode();
                if (hash < 0)
                    hash *= -1;
                _emailAuthService.FillingData(hash.ToString(), email, "cfgkhjesru645vcbnhbjtr");
                var result = await _emailAuthService.SignIn();
                if (!result)
                    Debug.Log("!!!AUTH!!! Fail auth");
                else
                {
                    Debug.Log("!!!AUTH!!! Auth OK");
                    _catalogManager.Initialize();
                }
            }
            else
            {
                Debug.Log($"!!!AUTH!!! Skip auth: PlayFabSettings.staticPlayer {(isStaticPlayerNull ? "==" : "!=")} null, client {(isClientLoggedIn ? "IS" : "is NOT")} logged");
            }
            CheckAuth();
        }
#endif
  }
}