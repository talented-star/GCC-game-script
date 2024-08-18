using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/IdleEnemyAction", fileName = "IdleEnemyAction", order = 51)]
    public class IdleEnemyAction : EnemyAction
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            entity.Agent.ResetPath();
            entity.AnimatorIdle();
        }
    }
}