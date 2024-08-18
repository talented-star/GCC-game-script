using GrabCoin.UI.ScreenManager;
using UnityEngine.SceneManagement;
using UnityEngine;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/GameLoadingScreen.prefab")]
    public class GameLoadingScreen : UIScreenBase
    {
        private PlayerScreensManager _screensManager;

        [Inject]
        private void Construct(
            PlayerScreensManager screensManager
            )
        {
            _screensManager = screensManager;
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void CheckOnEnable()
        {

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _screensManager.ClosePopup();
        }
    }
}
