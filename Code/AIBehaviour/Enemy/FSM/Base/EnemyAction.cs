using BehaviourSystem;
using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    public class EnemyAction : BehaviourAction<EnemyBehaviour>
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            //Debug.Log($"Start Action \"{GetType()}\" for \"{entity.gameObject.name}\"");
        }

        public override void DoAction(EnemyBehaviour entity)
        { }

        public override void DoFixAction(EnemyBehaviour entity)
        { }

        public override void DoLateAction(EnemyBehaviour entity)
        { }

        public override void EndAction(EnemyBehaviour entity)
        {
            //Debug.Log($"End Action \"{GetType()}\" for \"{entity.gameObject.name}\"");
        }
    }
}