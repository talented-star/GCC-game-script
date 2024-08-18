using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Actions/SubGraphAction", fileName = "SubGraphAction", order = 51)]
    public class SubGraphAction : EnemyAction
    {
        [SerializeField] private EnemyGraph _subGraphBehaviour;

#if UNITY_SERVER
        public override void BeginAction(EnemyBehaviour entity)
        {
            base.BeginAction(entity);
            var subController = new EnemyController();
            entity.AddSubController(name, subController);
            subController.TryInstall(entity, _subGraphBehaviour);
        }

        public override void DoAction(EnemyBehaviour entity)
        {
            base.DoAction(entity);
            entity.GetSubController(name).BehaviourUpdate();
        }

        public override void DoLateAction(EnemyBehaviour entity)
        {
            base.DoLateAction(entity);
            entity.GetSubController(name).LateBehaviourUpdate();
        }

        public override void DoFixAction(EnemyBehaviour entity)
        {
            base.DoFixAction(entity);
            entity.GetSubController(name).FixBehaviourUpdate();
        }

        public override void EndAction(EnemyBehaviour entity)
        {
            base.EndAction(entity);
            entity.GetSubController(name).TryUninstall();
            entity.RemoveSubController(name);
            //Debug.Log($"Stop subGraph {entity.gameObject.name}");
        }
#endif
    }
}