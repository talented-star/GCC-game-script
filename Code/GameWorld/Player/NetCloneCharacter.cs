using Cysharp.Threading.Tasks;
using GrabCoin.AIBehaviour;
using GrabCoin.Enum;
using GrabCoin.GameWorld.Player;
using GrabCoin.GameWorld.Weapons;
using GrabCoin.Services.Backend.Catalog;
using PlayFabCatalog;
using System;
using UnityEngine;
using static GrabCoin.GameWorld.Player.Player;

public class NetCloneCharacter : MonoBehaviour, IBattleEnemy
{
    [SerializeField] private Animator _animatorWings;
    [SerializeField] private Animator _animatorTail;
    private RuntimeAnimatorController _defaultController;
    private UnitMotor _character;
    private VRAvatarController _vrController;
    private WeaponBase _currentWeapon;
    private Animator _animator;
    private Vector3 _position;
    private Vector3 _rotation;
    private bool _isInitialize;
    private bool _isDie;
    private PlayerAnimation _playerAnimation;
    private CatalogManager _catalogManager;
    private CapsuleCollider _mainCollider;
    private string _playerName;

    public EnemyType GetTypeCreatures => EnemyType.Player;

    public Transform GetTransform { get { try { return (transform ?? null); } catch { return null; } } }

    public GameObject GetGameObject => gameObject;

    public Transform GetEyePoint => _animator.GetBoneTransform(HumanBodyBones.Head);

    public bool IsDie => _isDie;

    public float GetSize => _mainCollider.radius;

    public string GetName => _playerName;

    private void Awake()
    {
        _character = GetComponent<UnitMotor>();
        _vrController = GetComponent<VRAvatarController>();
        _animator = GetComponent<Animator>();
        _mainCollider = GetComponent<CapsuleCollider>();
        _defaultController = _animator.runtimeAnimatorController;
    }

    private void FixedUpdate()
    {
        if (!_isInitialize) return;

        transform.position = _position;//Vector3.MoveTowards(transform.position, _position, Time.fixedDeltaTime * 20f);
        transform.eulerAngles = _rotation;
    }

    public void InitializeMode(CatalogManager catalogManager, PlayerMode playerMode, Action<HitInfo, BodyPart> hitCallback)
    {
        _catalogManager = catalogManager;
        switch (playerMode)
        {
            case PlayerMode.ThirdPerson:
                _character.enabled = true;
                _vrController.SetAction(false);
                break;
            case PlayerMode.VR:
                _character.enabled = false;
                _vrController.SimulateMode = true;
                _vrController.SetAction(true);
                _vrController.Head.vrTarget = new GameObject("Head").transform;
                _vrController.LeftHand.vrTarget = new GameObject("LeftHand").transform;
                _vrController.RightHand.vrTarget = new GameObject("RightHand").transform;
                var eyePoint = new GameObject("Eyes");
                _vrController.SetEyes(eyePoint.transform);
                _vrController.IsReady = true;
                break;
        }
        SpawnWeaponInHand("");

        var hittable = GetComponentsInChildren<Hittable>();
        foreach (var hittableItem in hittable)
        {
            hittableItem.hitCallback += hitCallback;
            hittableItem.Owner = this;
        }

        _isInitialize = true;
    }

    public async void SpawnWeaponInHand(string weaponId)
    {
        if (_currentWeapon)
            Destroy(_currentWeapon.gameObject);
        if (_animator)
            _animator.runtimeAnimatorController = _defaultController;

        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        if (_catalogManager.GetItemData(weaponId) == null)
            await _catalogManager.CashingItem(weaponId);
        var weapon = _catalogManager.GetItemData(weaponId);
        //var weaponData = (weapon as EquipmentItem).customData;

        var weaponPrefab = Instantiate(weapon.itemConfig.Prefab, _animator.GetBoneTransform(HumanBodyBones.RightHand));
        weaponPrefab.transform.localPosition = Vector3.zero;
        weaponPrefab.transform.localRotation = Quaternion.identity;
        _currentWeapon = weaponPrefab.GetComponent<WeaponBase>();

        //_currentWeapon.SetAvailableForAttack(false);
        //_currentWeapon.Initialize(this, weaponData);

        //_currentWeapon.Initialize(this, weaponData);
        _currentWeapon.SetAvailableForAttack(true);

        if (_animator != null && (_currentWeapon?.AnimatorOverrideController))
            _animator.runtimeAnimatorController = _currentWeapon?.AnimatorOverrideController;
    }

    public void SetPosition(Transform position, Vector3 move, Vector3 direction, PlayerAnimation playerAnimation)
    {
        if (!_isInitialize) return;

        _character.SetCrouch((playerAnimation & PlayerAnimation.Crouch) == PlayerAnimation.Crouch);
        _character.SetHoldJump((playerAnimation & PlayerAnimation.Jump) == PlayerAnimation.Jump);
        _character.SetInput(new Vector2(move.x, move.z));
        _character.SetLookDirection(new Vector3(move.x, 0, move.z));

        Vector3 moveDir = transform.InverseTransformDirection(direction);
        _animator.SetFloat("Horizontal", moveDir.x, 0.1f, Time.deltaTime);
        _animator.SetFloat("Vertical", moveDir.z, 0.1f, Time.deltaTime);
        _animator.SetFloat("Forward", move.magnitude, 0.1f, Time.deltaTime);
        if (_animatorWings)
        {
            _animatorWings.SetFloat("Forward", move.magnitude, 0.1f, Time.deltaTime);
            _animatorWings.SetBool("IsOpen", (playerAnimation & PlayerAnimation.Levitation) == PlayerAnimation.Levitation);
        }
        if (_animatorTail)
            _animatorTail.SetFloat("Forward", move.magnitude, 0.1f, Time.deltaTime);
        _animator.SetFloat("Turn", -Vector3.SignedAngle(move, _character.transform.forward, Vector3.up) / 90f, 0.1f, Time.deltaTime);
        _animator.SetBool("Crouch", (playerAnimation & PlayerAnimation.Crouch) == PlayerAnimation.Crouch);
        _animator.SetBool("Aiming", (playerAnimation & PlayerAnimation.Aim) == PlayerAnimation.Aim);
        _animator.SetBool("IsLevitation", (playerAnimation & PlayerAnimation.Levitation) == PlayerAnimation.Levitation);
        _animator.SetBool("OnGround", _character.characterMovement.isGrounded);
        if (!_character.characterMovement.isGrounded)
            _animator.SetFloat("Jump", _character.characterMovement.velocity.y);
        var isOldDie = (_playerAnimation & PlayerAnimation.Die) == PlayerAnimation.Die;
        var isNewDie = (playerAnimation & PlayerAnimation.Die) == PlayerAnimation.Die;
        if (isOldDie != isNewDie)
            _animator.SetTrigger(isNewDie ? "Dead" : "Respawn");
        var isNewMining = (playerAnimation & PlayerAnimation.Mining) == PlayerAnimation.Mining;
        var isOldMining = (_playerAnimation & PlayerAnimation.Mining) == PlayerAnimation.Mining;
        if (isNewMining != isOldMining)
            _animator.SetTrigger(isNewMining ? "StartMine" : "EndMine");

        _position = position.position;
        _rotation = position.eulerAngles;
        _playerAnimation = playerAnimation;
    }

    public void SetVRPosition(Vector3 position, Vector3 target1, Vector3 target2, Vector3 target3)
    {
        _position = position;
        _vrController.Head.vrTarget.position = target1;
        _vrController.LeftHand.vrTarget.position = target2;
        _vrController.RightHand.vrTarget.position = target3;
    }

    public void SetVRRotation(Vector3 target1, Vector3 target2, Vector3 target3)
    {
        _vrController.Head.vrTarget.eulerAngles = target1;
        _vrController.LeftHand.vrTarget.eulerAngles = target2;
        _vrController.RightHand.vrTarget.eulerAngles = target3;
    }

    internal void StartMining()
    {
        _animator.SetTrigger("StartMine");
    }

    internal void StopMining()
    {
        _animator.SetTrigger("EndMine");
    }

    public void Shoot(GameObject netIdentity, Action<GameObject, HitbackInfo> callback, AttackData attackData)
    {
        _currentWeapon.Attack(netIdentity, callback, this, attackData);
    }

    public void SetLifeState(bool isDie)
    {
        _isDie = isDie;
        _animator.SetTrigger(isDie ? "Dead" : "Respawn");
    }

    internal void SetName(string playerName)
    {
        _playerName = playerName;
    }
}
