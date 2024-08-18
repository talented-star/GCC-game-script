using Cysharp.Threading.Tasks;
using GrabCoin.AIBehaviour.FSM;
using GrabCoin.GameWorld.Player;
using GrabCoin.GameWorld.Weapons;
using GrabCoin.Services.Backend.Catalog;
using Mirror;
using NaughtyAttributes;
using PlayFabCatalog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

namespace GrabCoin.AIBehaviour
{
    public class EnemyBehaviour : BaseEnemy, IBattleEnemy
#if UNITY_SERVER
        , IWeaponHandler
#endif
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator _animator;
        [SerializeField] protected WeaponBase _weaponSystem;
        [SerializeField] private AlarmList _alarmList;

        private Dictionary<string, EnemyController> _subControllers = new();
        private EnemyController _enemyController;
        private Queue<Vector3> _patrulPath;
        private Vector3 _currentMovePoint;
        private CatalogManager _catalogManager;

        public Action<BaseEnemy> _deathEnemy = null;
        public delegate void HomeLong(BaseEnemy baseEnemy);
        public HomeLong homeLong = null;
        public float attackLong;
        public float _groundFollowerTimer = 1f;
        private float _speedSurfaceRotation = 0.05f;

        public Animator Animator => _animator;
        public bool IsCompletePatrulPath { get; set; }
        public bool IsFinishedPatrulPath { get; set; }
        public float Timer { get; private set; }
        public Optionals<IBattleEnemy> TargetEnemy => _alarmList.GetTargetEnemy;
        public NavMeshAgent Agent => agent;
        public Vector3 PatrulPath
        {
            get => _patrulPath.Dequeue();
            set => _patrulPath.Enqueue(value);
        }

        //public float AttackLong => _attackLong;

        public EnemyType GetTypeCreatures => _defaultStats?.customData.enemyType ?? EnemyType.Passive;

        public Transform GetTransform => transform;

        public GameObject GetGameObject => gameObject;

        public Transform GetEyePoint => transform;

        public bool IsDie => _hittableInfo.health <= 0;

        public bool IsAlarm => _alarmList.IsAlarm;

        private bool _isUnderAttack;
        public bool IsUnderAttack
        {
            get
            {
                bool isAttacked = _isUnderAttack;
                _isUnderAttack = false;
                return isAttacked;
            }
        }

        private bool _isFinishAttack;
        public bool IsFinishAttack
        {
            get
            {
                bool isAttacked = _isFinishAttack;
                _isFinishAttack = false;
                return isAttacked;
            }
            set => _isFinishAttack = value;
        }

        public Transform CameraTransform { get => GetEyePoint; }

        public float GetSize => Agent.radius;

        public Vector3 CurrentMovePoint { get => _currentMovePoint; internal set => _currentMovePoint = value; }
        public Quaternion FromToRot { get; internal set; }

        public string GetName => nameEnemy;

        [SyncVar] public NameAction.Name CurrentAction;
        [SyncVar] private List<VisionParam> _eyeVisions;
        [SyncVar] private List<VisionParam> _attackVisions;
        [SyncVar(hook = nameof(OnChangeName))] public string nameEnemy;

        public bool HasEqualsGraph(XNode.NodeGraph nodeGraph, XNode.Node node)
        {
            if (_enemyController?.HasEqualsGraph(nodeGraph) ?? false)
                return _enemyController.HasEqualsNode(node);

            if (_subControllers.Count() > 0)
            {
                var controllers = _subControllers.Where(controller => controller.Value.HasEqualsGraph(nodeGraph));
                if (controllers.Count() > 0)
                    return controllers.First().Value.HasEqualsNode(node);
            }
            return false;
        }

        private void Awake()
        {
            _patrulPath = new();
#if !UNITY_SERVER
            var hittable = GetComponentsInChildren<Hittable>();
            foreach (var hittableItem in hittable)
            {
                hittableItem.Owner = this;
            }
#endif
        }

        private void OnChangeName(string _, string newName)
        {
            gameObject.name = newName;
        }

        public override IEnumerator EEnemyInitilization(CatalogManager catalogManager, EnemyItem defaultStats, Vector3 homePosition, List<float> taiming)
        {
            _eyeVisions = defaultStats.customData.enemyEyesight;
            _attackVisions = defaultStats.customData.attackZone;
            _isEnable = false;
            yield return null;

            _catalogManager = catalogManager;
#if UNITY_EDITOR
            if (_showEyesight)
            {
                ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_eyeVisions.ToArray()));
            }
            if (_showAttack)
                ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_attackVisions.ToArray()));
#endif

#if UNITY_SERVER
            _defaultStats = defaultStats;
            _alarmList = new AlarmList(this, ScriptableObject.CreateInstance<EnemyVision>().Init(_defaultStats.customData.enemyEyesight.ToArray()));
            this.homePosition = homePosition;
            agent.Warp(transform.position);

            _enemyController = new EnemyController();
            _enemyController.TryInstall(this, defaultStats.behaviour);

            transform.position = homePosition;

            _isEnable = true;
            _hittableInfo.health = _defaultStats.customData.health;

            var hittable = GetComponentsInChildren<Hittable>();
            foreach (var hittableItem in hittable)
            {
                hittableItem.hitCallback += TakeDamage;
                hittableItem.Owner = this;
            }

            if (_weaponSystem != null)
            {
                EquipmentCustomData attackInfo = new();
                attackInfo.damage = _defaultStats.customData.attackDamage;
                _weaponSystem.SetAvailableForAttack(false);
                _weaponSystem.Initialize(this, attackInfo); 
            }
#endif
        }

        public void RefreshStats(EnemyItem defaultStats)
        {
#if UNITY_SERVER
            _defaultStats = defaultStats;
            if (_weaponSystem != null)
            {
                var attackInfo = _weaponSystem.CustomData;
                attackInfo.damage = _defaultStats.customData.attackDamage;
                _weaponSystem.CustomData = attackInfo;
            }
#endif
        }

#if UNITY_SERVER
        internal void ReturnHomeArea()
        {
            _alarmList.ClearAlarm();
        }

        public override void Update()
        {
            base.Update();
            _enemyController?.BehaviourUpdate();
            _alarmList?.AlarmReduction(_defaultStats?.customData.timePursuit ?? 0);
        }

        public override void LateUpdate()
        {
            _enemyController?.LateBehaviourUpdate();
        }

        public override void TakeDamage(HitInfo hitInfo, BodyPart bodyPart)
        {
            if (_hittableInfo.health <= 0) return;


            base.TakeDamage(hitInfo, bodyPart);
            float damage = hitInfo.customData.ProceedDamage(bodyPart == BodyPart.Head);
            _alarmList.AddAlarmEnemy(hitInfo.battleEnemy, damage);
            _hittableInfo.health -= damage;
            AnimatorDamage();

            var hitBack = new HitbackInfo(bodyPart, damage, _hittableInfo.health / _defaultStats.customData.health);
            hitBack.classHitTarget = ClassHitTarget.Enemy;
            hitBack.isLastHit = false;
            if (_hittableInfo.health <= 0)
            {
                hitBack.isLastHit = true;
            }
            hitInfo.shooterCallback?.Invoke(hitInfo.netIdentity, hitBack);
        }

        public async override void EnemyDeath()
        {
            base.EnemyDeath();
            _enemyController.TryUninstall();
            _deathEnemy?.Invoke(this);

            agent.speed = 0f;
            float timeDie = 0;
            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Death")
                {
                    timeDie = clip.length;
                    Debug.Log($"Time death animation: {timeDie}");
                    break;
                }
            }
            await UniTask.Delay((int)(timeDie * 1000f));
            _isEnable = false;
            gameObject.SetActive(_isEnable);
        }

        public override void EnemyRespawn(EnemyItem defaultStats)
        {
            base.EnemyRespawn(defaultStats);
            _enemyController.TryInstall(this, defaultStats.behaviour);
            AnimatorReset();
            _alarmList.ClearAlarm();
            _isEnable = true;
            gameObject.SetActive(_isEnable);
            _hittableInfo.health = _defaultStats.customData.health;
        }
        
        public void AddSubController(string key, EnemyController enemySubController)
        {
            if (!_subControllers.ContainsKey(key))
                _subControllers.Add(key, enemySubController);
        }

        public void RemoveSubController(string key)
        {
            if (_subControllers.ContainsKey(key))
                _subControllers.Remove(key);
        }

        public EnemyController GetSubController(string key)
        {
            return _subControllers[key];
        }

        public bool IsDistanceToAttack()
        {
            _alarmList.UpdateAlarmEnemy();
            if (TargetEnemy.isInit)
            {
                float distance = Vector3.Distance(transform.position, TargetEnemy.Value.GetTransform.position);
                NavMeshAgent meshAgent = TargetEnemy.Value.GetGameObject.GetComponent<NavMeshAgent>();
                if (meshAgent != null)
                {
                    if (meshAgent.radius + agent.radius >= _defaultStats.customData.attackZone[0].distance)
                    {
                        distance -= meshAgent.radius;
                    }
                }
                return (distance <= _defaultStats.customData.attackZone[0].distance);
            }
            return false;
        }

        internal void MoveToTagetEnemy()
        {
            if (TargetEnemy.isInit)
                agent.SetDestination(TargetEnemy.Value.GetTransform.position);
        }

        internal void Attack()
        {
            _alarmList.AddAlarmEnemy(_alarmList.GetTargetEnemy.Value, 1);

            _weaponSystem.SetAvailableForAttack(true);

            AttackData data = new();
            data.attackParams = _defaultStats.customData.attackZone;
            _weaponSystem.Attack(null, default, this, data);
        }

        public void FindEnemyes()
        {
            Queue<Collider> copyList =
                new Queue<Collider>(Physics.OverlapSphere(transform.position, _defaultStats.customData.enemyEyesight[0].distance, _defaultStats.customData.enemyEyesight[0].mask, QueryTriggerInteraction.Collide));

            while (copyList.Count > 0)
            {
                Collider coll = copyList.Dequeue();

                if (!coll.TryGetComponent(out IBattleEnemy creature))
                    creature = coll.GetComponentInParent<IBattleEnemy>();

                if (creature != null && !creature.Equals(this) && !creature.IsDie)
                {
                    if (!_alarmList.ContainsEnemy(creature) &&
                        GetTypeCreatures >= EnemyType.Passive &&
                        creature.GetTypeCreatures < GetTypeCreatures)
                    {
                        if (Vision.IsVisibleUnit(creature, GetEyePoint, creature.GetTransform, ScriptableObject.CreateInstance<EnemyVision>().Init(_defaultStats.customData.enemyEyesight.ToArray())))
                            _alarmList.AddAlarmEnemy(creature);
                    }
                }
            }
        }

        public Vector3 TransformPoint(Vector3 attackPointOffset)
        {
            return transform.TransformPoint(attackPointOffset);
        }
#endif

        #region "Animations"
        [SyncVar(hook = nameof(SrvSetForward))] private float _forward;
        [SyncVar(hook = nameof(SrvSetNumAttack))] private int _numberAttack;
        private void SrvSetForward(float _, float newValue)
        {
            _animator.SetFloat("Forward", newValue);
        }
        public void SetNumAttack(int newValue)
        {
            _numberAttack = newValue;
        }
        private void SrvSetNumAttack(int _, int newValue)
        {
            _animator.SetInteger("countAttack", newValue);
        }
        [ClientRpc]
        public void SrvSetAttack()
        {
            _animator.SetTrigger("Attack");
            time = 0;
        }
        [ClientRpc]
        private void SrvResetAttack()
        {
            _animator.ResetTrigger("Attack");
            time = 0;
        }
        [ClientRpc]
        private void SrvSetDie()
        {
            _animator.SetTrigger("die");
            time = 0;
        }
        [ClientRpc]
        private void SrvSetHit()
        {
            _animator.SetTrigger("Hit");
        }
        [ClientRpc]
        private void SrvSetRoar()
        {
            _animator.SetTrigger("Roar");
        }
        [ClientRpc]
        private void SrvSetReset()
        {
            _animator.SetTrigger("Reset");
            time = 0;
        }

        public void AnimatorReset()
        {
            _animator.SetTrigger("Reset");
            SrvSetReset();
            _animator.ResetTrigger("Attack");
            SrvResetAttack();
            time = 0;
        }

        public void AnimatorStopAnim()
        {
            _animator.SetFloat("Forward", 0);
            time = 0;
            _forward = 0;
            agent.speed = 0f;
        }

        public void AnimatorIdle()
        {
            _animator.SetFloat("Forward", 0);
            _forward = 0;
            agent.speed = 0f;
        }

        public void AnimatorWalk()
        {
            _isEnable = true;
            _animator.SetFloat("Forward", 0.5f);
            _forward = 0.5f;
            _animator.ResetTrigger("Attack");
            SrvResetAttack();
            agent.speed = _defaultStats.customData.walkSpeed;
        }

        public void AnimatorRun()
        {
            _isEnable = true;
            _animator.SetFloat("Forward", 1);
            _forward = 1;
            _animator.ResetTrigger("Attack");
            SrvResetAttack();
            agent.speed = _defaultStats.customData.sprintSpeed;
        }

        public float AnimatorGetSpeed()
            => _animator.GetFloat("Forward");

        public void AnimatorDeath()
        {
            _animator.SetTrigger("die");
            SrvSetDie();
            EnemyDeath();
        }

        public void AnimatorDamage()
        {
            _animator.SetTrigger("Hit");
            SrvSetHit();
        }

        public void AnimatorRoar()
        {
            _animator.SetTrigger("Roar");
            SrvSetRoar();
            agent.speed = 0f;
        }

        public void LookAtTargetWithAnim()
        {
            var enemy = _alarmList.GetTargetEnemy;
            if (enemy.isInit)
            {
                agent.updateRotation = true;

                Vector3 lookPosition = (enemy.Value.GetTransform.position - transform.position).normalized;
                lookPosition.y = 0.0f;

                float angle = Vector3.Angle(lookPosition, transform.forward);
                if (angle > 15.0f)
                {
                    if (angle > 30.0f)
                    {
                        if (transform.InverseTransformDirection(lookPosition).x > 0)
                        {
                            _animator.SetBool("TurnRight", true);
                        }
                        else if (transform.InverseTransformDirection(lookPosition).x < 0)
                        {
                            _animator.SetBool("TurnLeft", true);
                        }
                    }
                    transform.forward = Vector3.Lerp(_animator.transform.forward, lookPosition, (Time.deltaTime * 13f));
                }
                else
                {
                    _animator.SetBool("TurnRight", false);
                    _animator.SetBool("TurnLeft", false);
                }
            }
        }

        public void LookAtTarget()
        {
            var enemy = _alarmList.GetTargetEnemy;
            if (enemy.isInit)
            {
                agent.updateRotation = true;

                Vector3 lookPos = enemy.Value.GetTransform.position - transform.position;

                Debug.DrawRay(transform.position, lookPos, Color.magenta);
                Debug.DrawRay(transform.position, transform.forward, Color.magenta);

                lookPos.y = 0;

                if (Vector3.Dot(lookPos, transform.forward) != 1)
                {
                    Quaternion lookRot = Quaternion.LookRotation(lookPos);
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, lookRot, Time.deltaTime * 13);
                }
            }
        }
        #endregion "Animations"

        #region "PatrulPath"
        public async void MoveNextPathPoint()
        {
            _currentMovePoint = PatrulPath;
            //agent.Move


            agent.SetDestination(_currentMovePoint);
            time = 0;
            await UniTask.NextFrame();
            if (agent.path == null || agent.path.corners.Length == 0)
            {
                MoveNextPathPoint();
            }
        }

        public void MovePathPoint()
        {
            if (agent.path != null && agent.path.corners.Length > 0)
            {
                Vector3 runDir = (transform.position - agent.path.corners[0]).normalized;
                //_currentMovePoint += runDir * Time.deltaTime;
                //agent.SetDestination(_currentMovePoint);
                agent.Move(runDir * Time.deltaTime);
                //transform.position += runDir * Time.deltaTime;

                for (int i = 1; i < agent.path.corners.Length; i++)
                {
                    Debug.DrawLine(agent.path.corners[i - 1], agent.path.corners[i]);
                }
            }
            else
                agent.SetDestination(_currentMovePoint);
        }

        internal void ClearPatrulPath()
        {
            _patrulPath?.Clear();
        }

        internal bool HasPatrulPath()
        {
            return (_patrulPath?.Count ?? 0) > 0;
        }

        [SerializeField] private float _deltaPosition;
        [SerializeField] private bool _approximatelyPosition;
        private Vector3 lastPoint;
        public float time;
        internal bool HasMoveToPointFinished()
        {
            _deltaPosition = (transform.position - lastPoint).magnitude;
            _approximatelyPosition = Mathf.Approximately(_deltaPosition, 0);
            if (_approximatelyPosition)
            {
                time += Time.deltaTime;
                if (time > 5)
                {
                    return true;
                }
            }
            lastPoint = transform.position;

            return !agent.hasPath || agent.isStopped || agent.remainingDistance <= agent.stoppingDistance;
        }
        #endregion "PatrulPath"

        #region "Timer"
        internal void StartTimer() =>
            Timer = 0f;

        internal void UpdateTimer(float delta) =>
            Timer += delta;
        #endregion "Timer"

#if UNITY_EDITOR
        [SerializeField] private bool _showEyesight;
        [SerializeField] private bool _showAttack;

        protected override void OnValidate()
        {
            base.OnValidate();
            HideEnemyEyesight();
            if (_showEyesight)
                ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_eyeVisions.ToArray()));
            if (_showAttack)
                ShowEnemyEyesight(ScriptableObject.CreateInstance<EnemyVision>().Init(_attackVisions.ToArray()));
        }

        public void ShowEnemyEyesight(EnemyVision vision/*, Material mat*/)
        {
            foreach (VisionParam v in vision.Visions)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                if (sphere.TryGetComponent(out Collider collider))
                    Destroy(collider);
                sphere.transform.SetParent(_animator.transform);
                sphere.transform.position = transform.position + Vector3.up;
                sphere.transform.localRotation = Quaternion.identity;
                //sphere.GetComponent<MeshRenderer>().material = mat;
                CastomizeThisMesh castomize = sphere.AddComponent<CastomizeThisMesh>();
                castomize.angle = v.angle;
                castomize.distance = v.distance;
                castomize.MyStart();

                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                if (sphere.TryGetComponent(out collider))
                    Destroy(collider);
                sphere.transform.SetParent(_animator.transform);
                sphere.transform.position = transform.position + Vector3.up;
                sphere.transform.localRotation = Quaternion.identity;
                //sphere.GetComponent<MeshRenderer>().material = mat;
                castomize = sphere.AddComponent<CastomizeThisMesh>();
                castomize.angle = v.angle;
                castomize.distance = v.distance;
                castomize.MyStart();
            }
        }

        public void HideEnemyEyesight()
        {
            foreach (var sectors in _animator.GetComponentsInChildren<CastomizeThisMesh>())
                Destroy(sectors.gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (_patrulPath == null) return;
            if (!HasMoveToPointFinished())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_currentMovePoint, 0.5f);
                Gizmos.DrawWireSphere(_currentMovePoint, 5f);
            }
            foreach (var point in _patrulPath)
            {
                bool isLast = point == _patrulPath.Last();
                Gizmos.color = isLast ? Color.green : Color.cyan;
                Gizmos.DrawSphere(point, 0.5f);
                Gizmos.DrawWireSphere(point, 5f);
            }
        }
#endif
    }
}
