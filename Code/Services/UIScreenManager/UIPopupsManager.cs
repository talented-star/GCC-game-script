using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace GrabCoin.UI.ScreenManager
{
    public class UIPopupsManager : MonoBehaviour
    {
        private DiContainer _diContainer;
        private UIScreensLoader _screensLoader;

        private ScreensCache cache = new ScreensCache();

        [Inject]
        public void Construct(DiContainer container, UIScreensLoader screensLoader)
        {
            _diContainer = container;
            _screensLoader = screensLoader;
        }

        public async UniTask<TScreen> Open<TScreen>() where TScreen : UIScreenBase
        {
            var instance = cache.TakeOrDefault<TScreen>();
            if (instance == null)
            {
                var prefab = await _screensLoader.Load<TScreen>();
                var go = _diContainer.InstantiatePrefab(prefab, transform);
                instance = go.GetComponent<TScreen>();
            }
            instance.gameObject.SetActive(true);
            SubscribeOnScreen(instance);
            instance.Transition.Show().Forget();
            //PlayerScreensManager.Instance?.OpenScreen(instance);
            return instance;
        }

        private async void CloseCalledHandler(UIScreenBase screen)
        {
            UnsubscribeFromScreen(screen);
            await screen.Transition.Hide();
            var type = screen.GetType();
            cache.Add(type, screen);
        }

        private async void ReleaseCalledHandler(UIScreenBase screen)
        {
            UnsubscribeFromScreen(screen);
            await screen.Transition.Hide();
            Destroy(screen.gameObject);
        }

        private void SubscribeOnScreen(UIScreenBase screen)
        {
            screen.OnCloseCalled += CloseCalledHandler;
            screen.OnReleaseCalled += ReleaseCalledHandler;
        }

        private void UnsubscribeFromScreen(UIScreenBase screen)
        {
            screen.OnCloseCalled -= CloseCalledHandler;
            screen.OnReleaseCalled -= ReleaseCalledHandler;
        }
    }
}