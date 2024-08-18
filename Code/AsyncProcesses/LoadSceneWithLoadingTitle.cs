using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using UnityEngine.SceneManagement;

namespace GrabCoin.AsyncProcesses
{
    public class LoadSceneWithLoadingTitle : IAsyncProcess<bool>
    {
        private LoadingOverlay _loadingOverlay;
        private string _scene;

        public LoadSceneWithLoadingTitle(string _scene, LoadingOverlay loadingOverlay)
        {
            this._scene = _scene;
            _loadingOverlay = loadingOverlay;
        }

        public async UniTask<bool> Run()
        {
            _loadingOverlay.Show();
            await UniTask.NextFrame();
            await SceneManager.LoadSceneAsync(_scene);
            _loadingOverlay.Hide();
            return true;
        }
    }
}