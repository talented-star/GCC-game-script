using BehaviourSystem;
using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM
{
    [NodeWidth(600), CreateNodeMenu("Enemy State")]
    public class EnemyState : BehaviourState<EnemyBehaviour> { }
}