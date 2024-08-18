using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GrabCoin.UI.ScreenManager.Transitions
{
    public class InstantTransition : ITransition
    {
        private GameObject _gameObject;
        public InstantTransition(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public async UniTask Show(ITransition prevOrNull)
        {
            if (prevOrNull != null)
                await prevOrNull.Hide();

            _gameObject.SetActive(true);
        }

        public async UniTask Hide()
        {
            _gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }
    }
}