using GrabCoin.AIBehaviour.FSM;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Enemys/Enemy Stats", fileName = "EnemyStats", order = 51)]
    public class EnemyStats : ScriptableObject
    {
        [SerializeField] private float health;
        [SerializeField] private float armor;
        [SerializeField] private float attackDamage;
        [SerializeField] private float pauseBetweenAttacks;
        [SerializeField] private int attackCountAnimation;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float timePursuit;
        [SerializeField] private float customUpdateTick;
        [SerializeField] private float idleTime;
        [SerializeField] private float alarmTime;
        [SerializeField] private float moveDistance;
        [SerializeField] private int pathLength;
        [SerializeField] private EnemyVision attackZone;
        [SerializeField] private EnemyVision enemyEyesight;
        [SerializeField] private EnemyGraph _behaviour;
        [SerializeField] private EnemyType enemyType;
        private LayerMask groundMask;
        [SerializeField] private string enemyPrefabPath;
        [SerializeField] private GameObject enemyPrefab;

        public float GetHealth => health;

        public float GetArmor => armor;

        public float GetAttackDamage => attackDamage;

        public float GetWalkSpeed => walkSpeed;

        public float GetSprintSpeed => sprintSpeed;

        public float GetIdleTime => idleTime;

        public float GetAlarmTime => alarmTime;

        public int GetPathLength => pathLength;

        public EnemyVision GetAttackZone => attackZone;

        public EnemyVision GetEnemyEyesight => enemyEyesight;

        public EnemyGraph GetBehaviour => _behaviour;

        public float TimePursuit => timePursuit;

        public string GetPrefabPath => enemyPrefabPath;

        public GameObject GetPrefab => enemyPrefab;

        public float GetUpdateTick => customUpdateTick;

        public EnemyType GetEnemyType => enemyType;

        public float PauseBetweenAttacks => pauseBetweenAttacks;

        public LayerMask GroundMask { get => groundMask; set => groundMask = value; }

        public float MoveDistance { get => moveDistance; set => moveDistance = value; }

        public int AttackCountAnimation => attackCountAnimation;
    }
}
