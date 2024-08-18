#if UNITY_EDITOR
using BehaviourSystem.Editor;
using UnityEngine;

namespace GrabCoin.AIBehaviour.FSM.Editor
{
    [CustomNodeEditor(typeof(EnemyState))]
    public class EnemyBehaviourStateEditor : BehaviourStateEditor<EnemyBehaviour> { }
}
#endif