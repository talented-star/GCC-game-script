using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/IsDieDecision", fileName = "IsDieDecision", order = 51)]
    public class IsDieDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.IsDie;
        }
    }
}