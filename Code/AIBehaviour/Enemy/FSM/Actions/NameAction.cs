using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/NameAction", fileName = "NameAction", order = 51)]
    public class NameAction : EnemyAction
    {
        public enum Name
        {
            Idle,
            FreeMove,
            Alarm,
            Pursut,
            Attack,
            Die
        }

        [SerializeField] private Name _nameAction;

        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            entity.CurrentAction = _nameAction;
        }
    }
}