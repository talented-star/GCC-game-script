using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/IsUnderAttackDecision", fileName = "IsUnderAttackDecision", order = 51)]
    public class IsUnderAttackDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.IsUnderAttack;
        }
    }
}