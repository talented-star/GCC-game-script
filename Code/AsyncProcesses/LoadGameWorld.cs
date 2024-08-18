using Cysharp.Threading.Tasks;
using GrabCoin.Config;
using GrabCoin.GameWorld.Network;
using GrabCoin.UI.ScreenManager;
using System;
using UnityEngine;

namespace GrabCoin.AsyncProcesses
{
    public class LoadGameWorld : IAsyncProcess<bool>
    {
        private string _scene;
        private string _ip;
        private LoadingOverlay _loadingOverlay;

        public LoadGameWorld(LoadingOverlay loadingOverlay)
        {
            _loadingOverlay = loadingOverlay;
        }

        public async UniTask<bool> Run()
        {
            Debug.Log("LoadGameWorld.Run() START");
            try
            {
                Debug.Log("LoadGameWorld.Run(): Stopping Client");
                PlayerNetworkManager.instance.StopClient();
                Debug.Log($"LoadGameWorld.Run(): Setting Network Address: IP = {_ip}, Port = {ScenePortConfig.GetPort(_scene)}");
                PlayerNetworkManager.instance.SetNetworkAddress(ScenePortConfig.GetIP(), ScenePortConfig.GetPort(_scene));
                Debug.Log($"LoadGameWorld.Run(): Loading Scene: {_scene}");
                await new LoadSceneWithLoadingTitle(_scene, _loadingOverlay).Run();
                Debug.Log("LoadGameWorld.Run(): Starting Client");
                PlayerNetworkManager.instance.StartClient();
                Debug.Log("LoadGameWorld.Run() FINISH true");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"LoadGameWorld.Run() ERROR: {ex.Message}\n{ex.StackTrace}");
                Debug.Log("LoadGameWorld.Run() FINISH false");
                return false;
            }
        }

        public LoadGameWorld SetLoadScene(string _scene, string ip)
        {
            this._scene = _scene;
            _ip = ip;
            return this;
        }
    }
}