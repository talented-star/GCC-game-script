using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace GrabCoin.UI.ScreenManager
{

    public class UIScreensManager : MonoBehaviour
    {

        private DiContainer _diContainer;
        private UIScreensLoader _screensLoader;

        private Dictionary<Type, UIScreenBase> _instances = new Dictionary<Type, UIScreenBase>();
        private Stack<UIScreenBase> _stack = new Stack<UIScreenBase>();

        private bool _transitionNow;

        [Inject]
        public void Construct(DiContainer container, UIScreensLoader screensLoader)
        {
            _diContainer = container;
            _screensLoader = screensLoader;
        }

        public async UniTask<TScreen> Open<TScreen>(bool leaveCurrentInStack = false) where TScreen : UIScreenBase
        {
            await WaitCurrentTransition();

            var prevScreen = CloseCurrent(leaveCurrentInStack);
            var screen = await GetScreenInstance<TScreen>();
            _stack.Push(screen);
            SwitchScreen(prevScreen, screen).Forget();
            //PlayerScreensManager.Instance?.OpenScreen(screen);
            return screen;
        }
        public async UniTask WaitCurrentTransition()
        {
            await UniTask.WaitUntil(() => !_transitionNow);
        }

        private async UniTask CloseCurrentAndShowPrev()
        {
            var prevScreen = CloseCurrent(false);
            var screen = GetLastFromStackOrNull();
            await SwitchScreen(prevScreen, screen);
        }

        private UIScreenBase CloseCurrent(bool leaveInStack)
        {
            if (_stack.IsEmpty)
                return null;

            var screen = leaveInStack ? _stack.Peek() : _stack.Pop();
            return screen;
        }

        private UIScreenBase GetLastFromStackOrNull()
        {
            if (_stack.IsEmpty)
                return null;
            var screen = _stack.Peek();
            return screen;
        }

        private async UniTask<TScreen> GetScreenInstance<TScreen>() where TScreen : UIScreenBase
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

            if (_stack.Peek() != screen)
                throw new Exception("Visible screen is not first element in stack()");
            await CloseCurrentAndShowPrev();
        }

        private async void ReleaseCalledHandler(UIScreenBase screen)
        {
            await WaitCurrentTransition();

            if (_stack.Contains(screen))
            {
                if (_stack.Peek() == screen)
                    await CloseCurrentAndShowPrev();
                else
                    _stack.Remove(screen);
            }
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
                await next.Transition.Show(prev != null ? prev.Transition : null);
            else
                await prev.Transition.Hide();
            _transitionNow = false;
        }
    }
}