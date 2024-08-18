using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/CalculatePathAction", fileName = "CalculatePathAction", order = 51)]
    public class CalculatePathAction : EnemyAction
    {
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            CalculateMovePoints(entity);
        }

        private async void CalculateMovePoints(EnemyBehaviour entity)
        {
            UnityEngine.Random.InitState(DateTime.UtcNow.Millisecond);

            entity.ClearPatrulPath();

            int count = 0;
            entity.IsCompletePatrulPath = false;
            while (count < entity.DefaultStats.customData.pathLength)
            {
                Vector3 offset = UnityEngine.Random.insideUnitSphere * entity.DefaultStats.customData.moveDistance;
                Vector3 originPos = entity.homePosition;
                originPos += new Vector3(offset.x, 0, offset.z);

                if (NavMesh.SamplePosition(originPos, out NavMeshHit hit, 20, NavMesh.AllAreas))
                //if (Physics.Raycast(entity.homePosition + Vector3.up * 50, hit.position, 200f))
                {
                    entity.PatrulPath = hit.position;
                    count++;
                }
                await UniTask.Delay(10);
            }
            entity.IsCompletePatrulPath = true;
        }

        private void CreateSpherePrimitive(Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = position;
            Destroy(go, 2);
        }
    }
}