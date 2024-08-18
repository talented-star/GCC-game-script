using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/AttackEnemyAction", fileName = "AttackEnemyAction", order = 51)]
    public class AttackEnemyAction : EnemyAction
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            AnimatorAttack(entity);
        }

        public override void DoAction(EnemyBehaviour entity)
        {
            base.DoAction(entity);
            entity.LookAtTarget();
        }

        private void AnimatorAttack(EnemyBehaviour entity)
        {
            entity.Agent.SetDestination(entity.transform.position);
            int rand = Random.Range(0, entity.DefaultStats.customData.attackCountAnimation);
            entity.Animator.SetInteger("countAttack", rand);
            entity.SetNumAttack(rand);
            entity.Animator.SetTrigger("Attack");
            entity.SrvSetAttack();
            entity.Agent.speed = 0f;
            foreach (AnimationClip clip in entity.Animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Attack_" + rand)
                    entity.attackLong = clip.length;
            }
        }
    }
}