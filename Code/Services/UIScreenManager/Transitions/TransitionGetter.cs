using System;
using UnityEngine;
using Zenject;

namespace GrabCoin.UI.ScreenManager.Transitions
{
    public class TransitionGetter : TransitionGetterBase
    {
        //After rename fields you need to edit custom editor TransitionChooserEditor
        [SerializeField] private TransitionSource _transitionSource;
        [SerializeField] private TransitionType _transitionType;

        public override ITransition Transition => _transition;

        private ITransition _transition;

        [Inject]
        private void Construct(TransitionsConstructor constructor)
        {
            if (_transitionSource == TransitionSource.DefaultForScreens)
                _transition = constructor.SetDefaultForScreensOn(gameObject);
            else if (_transitionSource == TransitionSource.DefaultForPopups)
                _transition = constructor.SetDefaultForPopupsOn(gameObject);
            else if (_transitionSource == TransitionSource.ManualSelect)
                _transition = constructor.SetTransitionOn(gameObject, _transitionType);
            else
                throw new Exception($"{typeof(TransitionSource).Name}.{_transitionSource} not implemented");
        }
    }
}