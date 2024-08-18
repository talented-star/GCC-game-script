using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/DistanceToTargetDecision", fileName = "DistanceToTargetDecision", order = 51)]
    public class DistanceToTargetDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
#if UNITY_SERVER
            return entity.IsDistanceToAttack();
#else
            return true;
#endif
        }
    }
}