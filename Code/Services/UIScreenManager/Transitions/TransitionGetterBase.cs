using System;
using UnityEngine;

namespace GrabCoin.UI.ScreenManager.Transitions
{
    public abstract class TransitionGetterBase : MonoBehaviour, ITransitionable
    {
        public abstract ITransition Transition { get; }
    }
}