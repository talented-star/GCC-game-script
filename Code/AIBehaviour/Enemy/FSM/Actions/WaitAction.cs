using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/WaitAction", fileName = "WaitAction", order = 51)]
    public class WaitAction : EnemyAction
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            entity.StartTimer();
        }

        public override void DoAction(EnemyBehaviour entity)
        {
            base.DoAction(entity);
            entity.UpdateTimer(Time.deltaTime);
        }
    }
}