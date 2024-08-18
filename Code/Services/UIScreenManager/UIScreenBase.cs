using GrabCoin.UI.ScreenManager.Transitions;
using System;
using UnityEngine;

namespace GrabCoin.UI.ScreenManager
{
    public abstract class UIScreenBase : MonoBehaviour
    {
        public Action<UIScreenBase> OnCloseCalled { get; set; }
        public Action<UIScreenBase> OnReleaseCalled { get; set; }

        public virtual ITransition Transition => GetTransition();

        private ITransition GetTransition()
        {
            if (_transition == null)
            {
                var control = GetComponent<TransitionGetterBase>();
                if (control == null)
                    throw new Exception($"{typeof(TransitionGetterBase)} not found on screen {name}");
                _transition = control.Transition;
            }
            return _transition;
        }
        private ITransition _transition;

        public virtual void CheckInputHandler(Controls controls) { }

        public abstract void CheckOnEnable();

        [ContextMenu("==CloseScreen==")]
        public virtual void Close()
        {
            OnCloseCalled?.Invoke(this);
            //PlayerScreensManager.Instance?.OpenScreen(null);
        }

        [ContextMenu("==ReleaseScreen==")]
        public void Release()
        {
            OnReleaseCalled.Invoke(this);
        }
    }
}