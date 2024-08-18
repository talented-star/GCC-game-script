using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/TimeAttackDecision", fileName = "TimeAttackDecision", order = 51)]
    public class TimeAttackDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.Timer >= entity.attackLong;
        }
    }
}