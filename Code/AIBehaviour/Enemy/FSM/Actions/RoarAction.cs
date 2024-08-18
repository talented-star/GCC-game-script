using Sources;
using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/RoarAction", fileName = "RoarAction", order = 51)]
    public class RoarAction : EnemyAction
    {
        [SerializeField] private string _roarSoundKey;
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            //AudioManager.Instance.PlaySound3D(_roarSoundKey, entity.transform.position);
        }
    }
}