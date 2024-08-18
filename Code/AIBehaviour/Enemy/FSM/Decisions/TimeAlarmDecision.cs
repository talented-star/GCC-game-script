using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/TimeAlarmDecision", fileName = "TimeAlarmDecision", order = 51)]
    public class TimeAlarmDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.Timer >= entity.DefaultStats.customData.alarmTime;
        }
    }
}