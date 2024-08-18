using GrabCoin.AIBehaviour.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Enemys/EnemyBehaviours", fileName = "EnemyBehaviours", order = 51)]
    public class EnemyBehaviours : ScriptableObject
    {
        [SerializeField] private List<DictionaryPair<EnemyType, EnemyGraph>> _behaviours;

        public EnemyGraph GetBehaviour(EnemyType type) =>
            _behaviours.FirstOrDefault(v => v.key == type).value;
    }

    [Serializable]
    public class DictionaryPair<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }
}
