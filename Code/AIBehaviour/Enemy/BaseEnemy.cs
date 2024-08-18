using GrabCoin.Services.Backend.Catalog;
using Mirror;
using PlayFabCatalog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    public abstract class BaseEnemy : NetworkBehaviour//, IEnemy, IDamage
    {
        [SyncVar(hook = nameof(EnemySetActive))] protected bool _isEnable = true;
        protected bool _flag = false;
        protected EnemyItem _defaultStats;
        [SerializeField] protected HittableInfo _hittableInfo = new();

        public bool isEnd = false;
        public bool isReady = false;

        public Vector3 homePosition;

        public bool Flag => _flag;
        public virtual bool isEnabled => _isEnable;

        public virtual bool IsEnemyDeath { get => false; }
        public EnemyItem DefaultStats => _defaultStats;

        public virtual void EnemyInitilization(EnemyItem defaultStats, Vector3 homePosition) { }
        public virtual void EnemyInit(EnemyItem defaultStats, Vector3 homePosition) { }
        public abstract IEnumerator EEnemyInitilization(CatalogManager catalogManager, EnemyItem defaultStats, Vector3 homePosition, List<float> taiming);
        public virtual void EnemyRespawn(EnemyItem defaultStats) { }

        public virtual void EnemyAwake() { }

        public virtual void Update() { }

        public virtual void LateUpdate() { }

        public virtual void FixedUpdate() { }

        public virtual void TakeDamage(HitInfo hitInfo, BodyPart bodyPart) { }

        public virtual void EnemySetActive(bool _, bool active)
        {
            _isEnable = active;
            gameObject.SetActive(_isEnable);
            if (isLocalPlayer)
            {
                //NetManager.SendEvent(CodeNetSend.AnimationAI
            }
        }

        public virtual void EnemyDestroy() => Destroy(gameObject);

        public virtual void EnemyDestroy(float time) => Destroy(gameObject, time);

        public virtual void EnemyDeath() { }
    }
}