using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace GrabCoin.GameWorld.Weapons
{
    public class HittableUnit : Hittable
    {
        [SerializeField] private float currentHealth;
        [SerializeField] private float currentShield;
        [SerializeField] public UnityEvent<HitInfo> onDead;

        private void Start()
        {
            currentHealth = _hittableInfo.health;
            currentShield = _hittableInfo.shield;
        }

        [Server]
        public override void Hit(HitInfo hitInfo)
        {
            if (currentHealth <= 0) return;

            float healthDamage = 0f;
            if (currentShield > 0)
            {
                currentShield = _hittableInfo.ProceedShieldDamage(hitInfo.customData.ProceedDamage(_bodyPart==BodyPart.Head), currentShield, out healthDamage);
                currentHealth -= healthDamage;
            }
            else
            {
                currentHealth = _hittableInfo.ProceedHealth(currentHealth, hitInfo.customData.ProceedDamage(_bodyPart == BodyPart.Head));
            }
            var hitBack = new HitbackInfo(_bodyPart, healthDamage, currentHealth);
            hitBack.isLastHit = false;
            hitBack.classHitTarget = ClassHitTarget.Enemy;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                hitBack.isLastHit = true;
                OnDead(hitInfo);
            }
            hitInfo.shooterCallback?.Invoke(hitInfo.netIdentity, hitBack);
        }

        public void OnDead(HitInfo hitInfo)
        {
            Debug.Log("DEAD: " + transform.name);
            onDead?.Invoke(hitInfo);
        }
    }
}