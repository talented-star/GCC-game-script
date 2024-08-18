using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/TrueDecision", fileName = "TrueDecision", order = 51)]
    public class TrueDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return true;
        }
    }
}