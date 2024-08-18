using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/MoveToPointFinishedDecision", fileName = "MoveToPointFinishedDecision", order = 51)]
    public class MoveToPointFinishedDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.HasMoveToPointFinished();
        }
    }
}