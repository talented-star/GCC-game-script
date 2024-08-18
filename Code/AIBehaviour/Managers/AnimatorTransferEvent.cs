using Sources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    public class AnimatorTransferEvent : MonoBehaviour
    {
        private EnemyBehaviour _enemyBehaviour;

        void Start()
        {
            _enemyBehaviour = GetComponentInParent<EnemyBehaviour>();
        }

        public void SetDamage() =>
            AttackEvent();

        public void AttackEvent()
        {
#if UNITY_SERVER
            _enemyBehaviour.Attack();
#endif
            AudioManager.Instance.PlaySound3D("enemyAttack", transform.position);
        }

        public void VoiceEvent()
        {
            //Debug.Log("Voice attack");
            //AudioManager.Instance.PlaySound3D("voiceAttack", transform.position);
        }

        public void FinishAttackEvent()
        {
            _enemyBehaviour.IsFinishAttack = true;
        }
    }
}