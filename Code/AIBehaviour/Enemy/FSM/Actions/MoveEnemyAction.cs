using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/MoveEnemyAction", fileName = "MoveEnemyAction", order = 51)]
    public class MoveEnemyAction : EnemyAction
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            entity.AnimatorWalk();
            MoveNextPathPoint(entity);
        }

        public async void MoveNextPathPoint(EnemyBehaviour entity)
        {
            entity.CurrentMovePoint = entity.PatrulPath;

            entity.Agent.SetDestination(entity.CurrentMovePoint);
            entity.time = 0;
            await UniTask.NextFrame();
            if (entity.Agent.path == null || entity.Agent.path.corners.Length == 0)
            {
                MoveNextPathPoint(entity);
            }
        }

        private void HitRaycast(EnemyBehaviour entity)
        {
            Ray[] rays = new Ray[2];
            rays[0] = new Ray(entity.Animator.transform.position, -Vector3.up * 5f);

            Vector3 point = entity.Animator.transform.position;
            point.x += entity.Agent.radius / 2;
            rays[1] = new Ray(point, -Vector3.up * 5f);

            Vector3 normal = Vector3.zero;
            for (int i = 0; i < rays.Length; i++)
            {
                if (NavMesh.Raycast(rays[i].origin, rays[i].origin + rays[i].direction * 5, out NavMeshHit hit, entity.Agent.areaMask))
                {
                    normal += hit.normal;
                }
            }

            entity.FromToRot = Quaternion.FromToRotation(Vector3.up, normal);
        }
    }
}