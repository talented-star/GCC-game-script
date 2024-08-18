using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/PathPatrulFinishedDecision", fileName = "PathPatrulFinishedDecision", order = 51)]
    public class PathPatrulFinishedDecision : EnemyDecision
    {
        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.HasPatrulPath();
        }
    }
}