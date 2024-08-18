using Cysharp.Threading.Tasks;
using GrabCoin.UI.Screens;
using GrabCoin.UI.ScreenManager;
using UnityEngine;

namespace GrabCoin.AsyncProcesses
{
    public class AuthProcess : IAsyncProcess<bool>
    {
        private UIScreensManager _screensManager;
        private bool _isEmulateAuth;

        public AuthProcess(UIScreensManager screensManager)
        {
            _screensManager = screensManager;
        }

        public async UniTask<bool> Run()
        {
            var screen = await _screensManager.Open<AuthScreen>();
            var result = _isEmulateAuth ? true : await screen.Process();
            screen.Close();
            await _screensManager.WaitCurrentTransition();

            Debug.Log($"[AuthProcess] Authorization complete: {result}");
            return result;
        }

        public AuthProcess SetLoadAuth(bool isEmulateAuth)
        {
            _isEmulateAuth = isEmulateAuth;
            return this;
        }
    }
}
