using GrabCoin.AsyncProcesses;
using GrabCoin.Config;
using GrabCoin.Services;
using GrabCoin.UI.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GrabCoin.Loader
{
    public class Bootstraper : MonoBehaviour
    {
        [SerializeField] private string _ipAddress;
        public bool _isDebug;
        [SerializeField] private string _ipDebugAddress;

        [SerializeField] private bool _isEmulateAuth;

        private DiContainer _container;
        public static string[] _scenes;

#if UNITY_EDITOR 
        private void Reset()
        {
            _scenes = SceneExt.GetScenes();
        }

        [ContextMenu("Update Scenes")]
        private void OnValidate()
        {
            _scenes = SceneExt.GetScenes();
        }
#endif

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private async void Start()
        {
            Debug.Log("Bootstraper.Start() START");
            Debug.Log("Bootstraper.Start() InitServices: Run()");
            await _container.Instantiate<InitServices>().Run();

            Debug.Log("Bootstraper.Start() PreloaderProcess: Run()");
            bool result = await _container.Instantiate<PreloaderProcess>().Run();
            Debug.Log($"Bootstraper.Start() result={result}");

#if !UNITY_EDITOR
            if (!result)
            {
                result = await _container.Instantiate<LauncherProcess>().Run();

                if (!result)
                    return;
            }
#endif

            Debug.Log("Bootstraper.Start() AcceptAgreementProcess: Run()");
            await _container.Instantiate<AcceptAgreementProcess>().Run();

            Debug.Log("Bootstraper.Start() LoadGameWorld: load scene");
            await _container.Instantiate<LoadGameWorld>().SetLoadScene(ScenePortConfig.GetLoadingScene(), _isDebug ? _ipDebugAddress : _ipAddress).Run();
            Debug.Log("Bootstraper.Start() FINISH");
        }
    }
}