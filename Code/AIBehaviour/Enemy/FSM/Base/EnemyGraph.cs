using BehaviourSystem;
using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [CreateAssetMenu(menuName = "Behaviour/Enemy/Graph", fileName = "NewEnemyGraph", order = 51)]
    public class EnemyGraph : BehaviourGraph<EnemyBehaviour> { }
}