using GrabCoin.UI.ScreenManager;
using UnityEngine;
using Zenject;

namespace GrabCoin.Testing.UI
{
    public class TestUISceneLoader : MonoBehaviour
    {
        private UIScreensManager _screensManager;
        private UIScreensLoader _screensLoader;

        [Inject]
        private void Construct(UIScreensManager manager, UIScreensLoader loader)
        {
            _screensManager = manager;
            _screensLoader = loader;
        }

        public void Start()
        {
            Test();
        }

        private async void Test()
        {
            await _screensLoader.Initialize();

            var screen = await _screensManager.Open<TestMainMenuScreen>();
        }
    }
}
