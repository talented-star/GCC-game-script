using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using UnityEngine;

namespace GrabCoin.AsyncProcesses
{
    public class InitServices : IAsyncProcess<bool>
    {
        private UIScreensLoader _screensLoader;

        public async UniTask<bool> Run()
        {
            await _screensLoader.Initialize();
            Debug.Log($"[UIScreensLoader] initialization complete");
            return true;
        }

        public InitServices(UIScreensLoader screensLoader)
        {
            _screensLoader = screensLoader;
        }
    }
}