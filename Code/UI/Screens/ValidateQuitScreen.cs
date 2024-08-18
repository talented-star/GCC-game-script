using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/ValidateQuitScreen.prefab")]
    public class ValidateQuitScreen : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _quitButton;

        private UniTaskCompletionSource<bool> _completion;
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
            _quitButton.onClick.AddListener(Quit);
            _backButton.onClick.AddListener(CloseScreen);
        }

        public override void CheckOnEnable()
        {

        }

        public UniTask<bool> Process()
        {
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        private void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        private void CloseScreen()
        {
            _screensManager.ClosePopup();
            _completion.TrySetResult(false);
        }
    }
}
