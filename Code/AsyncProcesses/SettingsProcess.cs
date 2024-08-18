using Cysharp.Threading.Tasks;
using GrabCoin.UI.Screens;
using GrabCoin.UI.ScreenManager;
using UnityEngine;

namespace GrabCoin.AsyncProcesses
{
    public class SettingsProcess : IAsyncProcess<bool>
    {
        private UIScreensManager _screensManager;

        public SettingsProcess(UIScreensManager screensManager)
        {
            _screensManager = screensManager;
        }

        public async UniTask<bool> Run()
        {
            var screen = await _screensManager.Open<MainMenuScreen>();
            var result = await screen.Process();
            screen.Close();
            await _screensManager.WaitCurrentTransition();

            Debug.Log($"[SelectGameModeProcess] Selected mode complete: {result}");
            return result;
        }
    }
}
