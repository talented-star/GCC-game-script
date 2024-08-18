using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Decision/TimeoutDecision", fileName = "TimeoutDecision", order = 51)]
    public class TimeoutDecision : EnemyDecision
    {
        [SerializeField] private float _waitTime;

        public override bool GetDecision(EnemyBehaviour entity)
        {
            return entity.Timer >= _waitTime;
        }
    }
}