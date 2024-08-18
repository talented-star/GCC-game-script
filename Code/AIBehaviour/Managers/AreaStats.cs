using GrabCoin.AIBehaviour.FSM;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Enemys/Area Stats", fileName = "AreaStats", order = 51)]
    public class AreaStats : ScriptableObject
    {
        public string enemyKey;
        [Range(50, 1000)]
        public float _patrulDistance = 50f;
        [Range(50, 1000)]
        public float _pursuitDistance = 80f;
        public float _reSpawnInterval;
        public int _maxEnemys;
        public LayerMask _groundMask;
    }
}
