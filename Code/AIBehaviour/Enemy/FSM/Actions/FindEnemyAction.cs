using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/FindEnemyAction", fileName = "FindEnemyAction", order = 51)]
    public class FindEnemyAction : EnemyAction
    {
        public override void DoAction(EnemyBehaviour entity)
        {
            base.DoAction(entity);
#if UNITY_SERVER
            entity.FindEnemyes();
#endif
        }
    }
}