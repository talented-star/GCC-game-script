using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/PathCalculateCompleteDecision", fileName = "PathCalculateCompleteDecision", order = 51)]
    public class PathCalculateCompleteDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.IsCompletePatrulPath;
        }
    }
}