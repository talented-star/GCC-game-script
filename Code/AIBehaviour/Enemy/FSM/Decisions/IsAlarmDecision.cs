using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/IsAlarmDecision", fileName = "IsAlarmDecision", order = 51)]
    public class IsAlarmDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.IsAlarm;
        }
    }
}