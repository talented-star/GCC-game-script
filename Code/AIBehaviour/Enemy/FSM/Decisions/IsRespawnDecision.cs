using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/IsRespawnDecision", fileName = "IsRespawnDecision", order = 51)]
    public class IsRespawnDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return !entity.IsDie;
        }
    }
}