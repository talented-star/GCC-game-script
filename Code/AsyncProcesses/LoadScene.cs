using Cysharp.Threading.Tasks;
using GrabCoin.Services;
using UnityEngine.SceneManagement;

namespace GrabCoin.AsyncProcesses
{
    //TODO Borodin Потестировать загрузку сцены через корутину!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public class LoadScene : IAsyncProcess<bool>
    {
        private string _scene;

        public LoadScene(string sceneName)
        {
            _scene = sceneName;
        }

        public async UniTask<bool> Run()
        {
            await SceneManager.LoadSceneAsync(_scene);
            return true;
        }
    }
}