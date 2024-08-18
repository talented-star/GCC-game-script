using Cysharp.Threading.Tasks;
using GrabCoin.AIBehaviour;
using GrabCoin.GameWorld.Player;
using GrabCoin.GameWorld.Resources;
using Mirror;
using System;
using UnityEngine;

namespace GrabCoin.GameWorld.Weapons
{
    public class Multitool : WeaponBase
    {
        [SerializeField] protected Transform _attackPoint;
        [SerializeField] protected float _digDistance = 3f;
        [SerializeField] protected LayerMask _ignoreLayer;

        private Player.Player _player;
        private ThirdPersonPlayerController _playerController;
        private async UniTaskVoid PlayerSpawned()
        {
            await UniTask.WaitUntil(() => NetworkClient.localPlayer?.gameObject != null);
            _player = NetworkClient.localPlayer.gameObject.GetComponent<Player.Player>();
            _player.GetPlayer().TryGetComponent(out _playerController);
        }

        private void Start()
        {
            PlayerSpawned().Forget();
        }

        internal override void Attack(GameObject netIdentity, Action<GameObject, HitbackInfo> callback, IBattleEnemy battleEnemy, AttackData attackData = default)
        {
            if (_weaponHandler != null)
            {
                _attackData.originPosition = /*_attackPoint.position;*/ _weaponHandler.CameraTransform.position;
                _attackData.direction = /*_attackPoint.forward; _spreadData.GetSpreadAngle(ProceedSpread()) **/ _weaponHandler.CameraTransform.forward;
                _attackData.ignoreMask = _ignoreLayer;
                _attackData.customData = CustomData;
                Translator.Send(PlayerNetworkProtocol.Attack, _attackData);

                muzzleArkFlash.transform.localEulerAngles = Vector3.zero;
                Ray rayShot = new Ray(_attackData.originPosition, _attackData.direction);
                if (Physics.Raycast(rayShot, out var hit, 100f, ~_ignoreLayer, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform.parent.TryGetComponent(out MiningResource miningResource))
                    {
                        float newWeght = InventoryScreenManager.Instance.CurrentWeight + miningResource.GetWeight();
                        if (newWeght > _player.InventoryLimit)
                            Translator.Send(HUDProtocol.HelperInfoWithTime,
                                new StringData { value = miningResource.GetName() + "\n" + "Inventory full" });
                        else if (hit.distance <= _attackData.customData.shootDistance)
                        {
                            _playerController.UseTarget(miningResource, false);
                            CreateMinedDecale(miningResource.transform);
                        }
                    }
                    muzzleArkFlash.transform.forward = hit.point - muzzleArkFlash.transform.position;
                }
            }

            muzzleArkFlash.LaunchRay();
        }

        private void CreateMinedDecale(Transform parent)
        {
            Translator.Send(HUDProtocol.CreateUIEffect, new DamageEffectData { isCrit = false, damage = CustomData.damage.ToString("F2") });

            Ray rayShot = new Ray(_weaponHandler.CameraTransform.position, _weaponHandler.CameraTransform.forward);
            if (Physics.Raycast(rayShot, out var hit, CustomData.shootDistance, ~_ignoreLayer, QueryTriggerInteraction.Ignore)) { }

            GameObject test = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            test.transform.localScale = Vector3.one * 0.03f;
            test.transform.SetParent(parent);
            test.transform.position = hit.point;
            if (test.TryGetComponent(out Collider collider))
                Destroy(collider);
            Destroy(test, 0.5f);
        }

        private void OnDrawGizmos()
        {
            if (_attackPoint != null)
            {
                Gizmos.DrawRay(_attackPoint.position, _attackPoint.forward * _digDistance);
            }
        }
    }

}