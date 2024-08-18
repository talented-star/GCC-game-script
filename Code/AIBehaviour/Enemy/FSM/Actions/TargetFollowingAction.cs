using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/TargetFollowingAction", fileName = "TargetFollowingAction", order = 51)]
    public class TargetFollowingAction : EnemyAction
    {
        [SerializeField] private bool _isWithAnimation;
        public override void DoAction(EnemyBehaviour entity)
        {
            base.DoAction(entity);
            if (_isWithAnimation)
                entity.LookAtTargetWithAnim();
            else
                entity.LookAtTarget();
        }
    }
}