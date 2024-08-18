using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/MoveToTargetEnemyAction", fileName = "MoveToTargetEnemyAction", order = 51)]
    public class MoveToTargetEnemyAction : EnemyAction
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            entity.AnimatorRun();
#if UNITY_SERVER
            entity.MoveToTagetEnemy();
#endif
        }
    }
}