using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace GrabCoin.UI.ScreenManager
{
    public class UIGameScreensManager : MonoBehaviour
    {
        private DiContainer _diContainer;
        private UIScreensLoader _screensLoader;

        private Dictionary<Type, UIScreenBase> _instances = new Dictionary<Type, UIScreenBase>();
        private UIScreenBase _currentUI;
        //private Stack<UIScreenBase> _stack = new Stack<UIScreenBase>();

        private bool _transitionNow;

        [Inject]
        public void Construct(DiContainer container, UIScreensLoader screensLoader)
        {
            _diContainer = container;
            _screensLoader = screensLoader;
        }

        public void RegisterScreen<TScreen>(TScreen screen) where TScreen : UIScreenBase
        {
            var type = typeof(TScreen);
            if (!_instances.TryGetValue(type, out var instance))
            {
                var go = _diContainer.Bind<TScreen>().FromInstance(screen).AsSingle().NonLazy();
                _instances.Add(type, screen);
                SubscribeOnScreen(screen);
            }
        }

        public async UniTask<TScreen> Open<TScreen>() where TScreen : UIScreenBase
        {
            await WaitCurrentTransition();

            var newScreen = await GetScreenInstance<TScreen>();
            SwitchScreen(_currentUI, newScreen).Forget();
            //PlayerScreensManager.Instance?.OpenScreen(newScreen);
            return newScreen;
        }

        public async UniTask WaitCurrentTransition()
        {
            await UniTask.WaitUntil(() => !_transitionNow);
        }

        private async UniTask CloseCurrentAndShowPrev()
        {
            var screen = GetLastFromStackOrNull();
            await SwitchScreen(_currentUI, screen);
        }

        private UIScreenBase GetLastFromStackOrNull()
        {
            return _currentUI;
        }

        public async UniTask<TScreen> GetScreenInstance<TScreen>() where TScreen : UIScreenBase
        {
            var type = typeof(TScreen);
            if (_instances.TryGetValue(type, out var instance) == false)
            {
                var prefab = await _screensLoader.Load<TScreen>();
                var go = _diContainer.InstantiatePrefab(prefab, transform);
                instance = go.GetComponent<UIScreenBase>();
                _instances.Add(type, instance);
                SubscribeOnScreen(instance);
            }
            return (TScreen)instance;
        }

        private async void CloseCalledHandler(UIScreenBase screen)
        {
            await WaitCurrentTransition();

            //if (_currentUI != screen)
            //    throw new Exception("Visible screen is not first element in stack()");
            //await CloseCurrentAndShowPrev();
            await SwitchScreen(screen, null);
        }

        private async void ReleaseCalledHandler(UIScreenBase screen)
        {
            await WaitCurrentTransition();

            if (_currentUI == screen)
                await CloseCurrentAndShowPrev();

            var pair = _instances.Where(pair => pair.Value == screen).First();
            _instances.Remove(pair.Key);
            UnsubscribeFromScreen(screen);
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

        private async UniTask SwitchScreen(UIScreenBase prev, UIScreenBase next)
        {
            _transitionNow = true;
            if (next != null)
            {
                await next.Transition.Show(prev != null ? prev.Transition : null);
            }
            else
            {
                await prev.Transition.Hide();
            }
            _currentUI = next;
            _transitionNow = false;
        }
    }
}