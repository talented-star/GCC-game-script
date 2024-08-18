using Cysharp.Threading.Tasks;
using GrabCoin.UI.Screens;
using GrabCoin.UI.ScreenManager;
using UnityEngine;

namespace GrabCoin.AsyncProcesses
{
    public class AcceptAgreementProcess : IAsyncProcess<bool>
    {
        private PlayerScreensManager _screensManager;

        public async UniTask<bool> Run()
        {
            var screen = await _screensManager.OpenScreen<LoginScreen>();
            var result = await screen.Process();
            screen.Release();
            await _screensManager.WaitCurrentTransition();
            Debug.Log($"[AcceptAgreementProcess] agreement accepted: {result}");
            return result;
        }

        public AcceptAgreementProcess(PlayerScreensManager screensManager)
        {
            _screensManager = screensManager;
        }
    }
}