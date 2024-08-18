using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/SettingsScreen.prefab")]
    public class SettingsScreen : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _displayButton;
        [SerializeField] private Button _graphicsButton;
        [SerializeField] private Button _inputButton;
        [SerializeField] private Button _profileButton;

        [SerializeField] private GameObject _displayPanel;
        [SerializeField] private GameObject _graphicsPanel;
        [SerializeField] private GameObject _inputPanel;
        [SerializeField] private GameObject _profilePanel;

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
            _backButton.onClick.AddListener(() => Next());
            _displayButton.onClick.AddListener(() => OpenPanel(_displayPanel));
            _graphicsButton.onClick.AddListener(() => OpenPanel(_graphicsPanel));
            _inputButton.onClick.AddListener(() => OpenPanel(_inputPanel));
            _profileButton.onClick.AddListener(() => OpenPanel(_profilePanel));
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        private UIScreenBase _backScreen;

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
            {
                var backScreenType = _backScreen.GetType();

                //MethodInfo openScreenMethod = typeof(PlayerScreensManager).GetMethod("OpenScreen");
                //openScreenMethod.MakeGenericMethod(backScreenType).Invoke(this, null);

                if (SceneManager.GetActiveScene().buildIndex == 1)
                    _screensManager.OpenScreen<MainMenuScreen>().Forget();
                else
                    _screensManager.OpenScreen<InGameMenu>().Forget();
            }
        }

        //private void GetGenericMethod<T>(T obj) where T : class
        //{
        //    MethodInfo method = typeof(_screensManager.GetType()).GetMethod("OpenScreen", BindingFlags.Static | BindingFlags.Public);
        //    Type[] genericArguments = new Type[] { obj.GetType() };
        //    MethodInfo genericMethod = method.MakeGenericMethod(genericArguments);
        //    genericMethod.Invoke(null, null);
        //}
        public UniTask<bool> Process(UIScreenBase backScreen)
        {
            _backScreen = backScreen;
            _completion = new UniTaskCompletionSource<bool>();
            //XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            return _completion.Task;
        }

        private void Next()
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
                _screensManager.OpenScreen<MainMenuScreen>().Forget();
            else
                _screensManager.OpenScreen<InGameMenu>().Forget();
        }

        private void OpenPanel(GameObject panel)
        {
            CloseAllPanel();
            panel.SetActive(true);
        }

        private void CloseAllPanel()
        {
            _displayPanel.SetActive(false);
            _graphicsPanel.SetActive(false);
            _inputPanel.SetActive(false);
            _profilePanel.SetActive(false);
        }
    }
}