using Sources;
using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/DieAction", fileName = "DieAction", order = 51)]
    public class DieAction : EnemyAction
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            entity.AnimatorDeath();
        }
    }
}