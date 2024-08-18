using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/IsFinishAttackDecision", fileName = "IsFinishAttackDecision", order = 51)]
    public class IsFinishAttackDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.IsFinishAttack;
        }
    }
}