using GrabCoin.UI.ScreenManager.Transitions;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.ScreenManager
{
    [CreateAssetMenu(fileName = "UITransitionsConstructor", menuName = "UIScreenManager/UITransitionsConstructor", order = 1)]
    public class TransitionsConstructor:ScriptableObject 
    {
        [SerializeField] private Image StretchedImagePrefab;
        [SerializeField] private TransitionType DefaultForScreens;
        [SerializeField] private TransitionType DefaultForPopups;

        public ITransition SetTransitionOn(GameObject go, TransitionType transitionType)
        {
            if (transitionType == TransitionType.Instant)
                return SetInstantTransition(go);
            else if (transitionType == TransitionType.ToBlack)
                return SetTransitionToBlack(go);
            else if (transitionType == TransitionType.ToTransparent)
                return SetTransitionToTransparent(go);
            else
                throw new System.Exception($"Transition {transitionType} not implemented");
        }

        public ITransition SetDefaultForScreensOn(GameObject go) => SetTransitionOn(go, DefaultForScreens);
        public ITransition SetDefaultForPopupsOn(GameObject go) => SetTransitionOn(go, DefaultForPopups);

        private ITransition SetInstantTransition(GameObject go) => new InstantTransition(go);
        private ITransition SetTransitionToTransparent(GameObject go) => new TransitionToTransparent(go, StretchedImagePrefab);
        private ITransition SetTransitionToBlack(GameObject go) => new TransitionToBlack(go, StretchedImagePrefab);
    }
}