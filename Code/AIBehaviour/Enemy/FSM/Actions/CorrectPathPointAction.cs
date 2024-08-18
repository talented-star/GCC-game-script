using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/CorrectPathPointAction", fileName = "CorrectPathPointAction", order = 51)]
    public class CorrectPathPointAction : EnemyAction
    {
        public override void DoAction(EnemyBehaviour entity)
        {
            base.DoAction(entity);
            entity.MovePathPoint();
        }
    }
}