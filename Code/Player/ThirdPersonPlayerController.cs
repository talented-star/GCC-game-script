using EasyCharacterMovement;
using GrabCoin.AIBehaviour.FSM;
using GrabCoin.GameWorld.Resources;
using GrabCoin.UI.ScreenManager;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Device;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.GameWorld.Player
{
    public class ThirdPersonPlayerController : MonoBehaviour
    {
        [SerializeField] private UnitMotor _unitMotor;
        [SerializeField] private ThirdPersonCameraController _cameraController;
        [SerializeField] private MultiAimConstraint _multiAimBody1Constraint;
        [SerializeField] private MultiAimConstraint _multiAimBody2Constraint;
        [SerializeField] private MultiAimConstraint _multiAimLeftHandConstraint;
        [SerializeField] private TwoBoneIKConstraint _multiParentConstraint;
        [SerializeField] private Animator _animator;
        [SerializeField] private Animator _animatorHead;
        [SerializeField] private Animator _animatorWings;
        [SerializeField] private Animator _animatorTail;

        [Header("Character")]
        [SerializeField] private float _aimSpeed = 2f;
        [SerializeField] private float groundedDelay = 0.2f;

        [Header("Camera")]

        [Header("Default")]
        [SerializeField] private float _defaultDistance = 4.5f;
        [SerializeField] private Vector2 _defaultOffset = new Vector2(1f, 0.5f);

        [Header("Aiming")]
        [SerializeField] private float _aimingDistance = 1.5f;
        [SerializeField] private Vector2 _aimingOffset = new Vector2(3.25f, 0.75f);
        [SerializeField, Range(0f, 1f)] private float _aimingWeight = 0.85f;
        [SerializeField, Range(0f, 1f)] private float _aimingBodyWeight = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _aimingHeadWeight = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _aimingClampWeight = 0.5f;
        [SerializeField] private bool _constantAiming = false;
        //[SerializeField] private InventoryScreenManager inventoryScreen;

        public UnitMotor UnitMotor
        {
            get => _unitMotor;
        }

        private Controls _controls;
        private Player _player;
        private bool _bufferIsDie;
        private bool _isKeyboardMode;

        public event Action StartTalk;
        public event Action Talked;
        private CustomEvent _onChatState;
        private CustomSignal _onInteract;
        private PlayerState _playerState;
        private UIScreensManager _screensManager;
        private bool _isSafeble;
        private float _groundedDelayTimer = 0;

        public bool IsSafeble => _isSafeble;

        [Inject]
        private void Construct(Controls controls, PlayerState playerState, UIScreensManager screensManager)
        {
            _controls = controls;
            _playerState = playerState;
            _screensManager = screensManager;
            _controls.Player.Jump.started += JumpStarted;
        }

        //private void Awake()
        //{
        //    var manager = new GameObject();
        //    manager.AddComponent<ScreenOverlayManager>();
        //}

        private void Start()
        {
            _onChatState = OnChatState;
            Translator.Add<UIPlayerProtocol>(_onChatState);
            _onInteract = OnInteract;
            Translator.Add<PlayerNetworkProtocol>(_onInteract);
        }

        private void OnDestroy()
        {
            Translator.Remove<UIPlayerProtocol>(_onChatState);
            Translator.Remove<PlayerNetworkProtocol>(_onInteract);
        }

        private void OnInteract(System.Enum codeEvent)
        {
            switch (codeEvent)
            {
                case PlayerNetworkProtocol.Interact:
                    CheckInteract();
                    break;
            }
        }

        private void OnChatState(System.Enum codeEvent, ISendData data)
        {
            switch (codeEvent)
            {
                case UIPlayerProtocol.StateChat:
                case UIPlayerProtocol.OpenGameUI:
                    var state = (BoolData)data;
                    //Debug.Log($"<color=red>Cursor visible: {state.value}</color>");
                    _isKeyboardMode = state.value;
                    Cursor.lockState = state.value ? CursorLockMode.None : CursorLockMode.Locked;
                    Cursor.visible = state.value;
                    if (_unitMotor.isAiming)
                        _unitMotor.SetAiming(!_isKeyboardMode);
                    break;
            }
        }

        Vector3 aimPosition;
        public void SetWeaponHandPoints(Vector3ArrayData handPointData)
        {
            _multiAimLeftHandConstraint.data.sourceObjects.First().transform.position = handPointData.value[0];
            _multiParentConstraint.data.target.transform.position = handPointData.value[1];
            _multiParentConstraint.data.target.transform.eulerAngles = handPointData.value[2];
        }

        public void SetWeaponFollower(Transform follower)
        {
            
        }

        private void JumpStarted(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _groundedDelayTimer = groundedDelay;
                _unitMotor.SetJumpDown();
            }
        }

        void Update()
        {
            if (_player == null) return;
            UpdateController();
            UpdateAnimator();
            if (_player != null && _player.isLocalPlayer)
            {
                var anim = Player.PlayerAnimation.None;
                if (_unitMotor.crouch)
                    anim |= Player.PlayerAnimation.Crouch;
                if (_unitMotor.holdJump)
                    anim |= Player.PlayerAnimation.Jump;
                if (_unitMotor.mining)
                    anim |= Player.PlayerAnimation.Mining;
                if (_unitMotor.isAiming)
                    anim |= Player.PlayerAnimation.Aim;
                if (_unitMotor._isLevitating)
                    anim |= Player.PlayerAnimation.Levitation;
                _player.CmdSetNetworkMoveVars(transform.position, _unitMotor.movementDirection * (_unitMotor.isRunning ? 1 : 0.5f), _unitMotor.movementDirection, anim);
            }
        }

		public ThirdPersonCameraController GetCameraController()
        {
            return _cameraController;
        }
        private void UpdateController()
        {
            if (_controls.Player.PushToTalk.WasPressedThisFrame())
            {
                StartTalk?.Invoke();
            }

            if (_controls.Player.PushToTalk.IsPressed())
            {
                Talked?.Invoke();
            }
            
            if (UnitMotor.characterMovement.isOnGround && Vector3.Angle(-UnitMotor.gravity.normalized, UnitMotor.characterMovement.groundNormal) >= UnitMotor.characterMovement.slopeLimit || !UnitMotor.IsGrounded()) 
            {
                _groundedDelayTimer += Time.deltaTime;
            }
            else if (UnitMotor.IsGrounded())
            {
                _groundedDelayTimer = 0;
            }

            if (_bufferIsDie) return;
            Vector2 input = _isKeyboardMode ? Vector2.zero : _controls.Player.Move.ReadValue<Vector2>();

            if (!_isKeyboardMode)
            {
                if (!_isSafeble && (_controls.Player.Aim.IsPressed() || _constantAiming))
                {
                    _unitMotor.SetLookDirection(Vector3.forward.relativeTo(_cameraController.transform));
                    _cameraController.SetTargetDistance(_aimingDistance);
                    _cameraController.SetTargetOffset(_aimingOffset);
                }
                else
                {
                    if (UnitMotor._isLevitating)
                    {
                        _unitMotor.SetLookDirection(UnitMotor.characterMovement.velocity.onlyXZ());
                    }
                    else
                    {
                        _unitMotor.SetLookDirection(new Vector3(input.x, 0, input.y).relativeTo(_cameraController.transform));
                    }
                    _cameraController.SetTargetDistance(_defaultDistance);
                    _cameraController.SetTargetOffset(_defaultOffset);
                }
                if (_controls.Player.Dash.WasPerformedThisFrame() && _unitMotor.IsDash)
                {
                    if (_player.CanGetStamina(_player.CostAbility))
                        _unitMotor.Dash(new Vector3(input.x, 0, input.y).relativeTo(_cameraController.transform));
                }
                if (!_isSafeble)
                    _unitMotor.SetAiming(_controls.Player.Aim.IsPressed() || _constantAiming);
            }

            bool canRun = false;
            if (!_controls.Player.Crouch.IsPressed() && _controls.Player.SlowDown.IsPressed() && !Mathf.Approximately(input.magnitude, 0))
                canRun = _player.CanGetStamina(_player.CostRun * Time.deltaTime);

            _unitMotor.SetRun(_controls.Player.SlowDown.IsPressed() && !_isKeyboardMode && canRun);
            _unitMotor.SetCrouch(_controls.Player.Crouch.IsPressed() && !_isKeyboardMode);
            if (_unitMotor.isLevitateActivity)
                _unitMotor.SetHoldJump(_controls.Player.Jump.IsPressed() && !_isKeyboardMode && _player.CanGetStamina(_player.CostAbility * Time.deltaTime));
            _unitMotor.SetInput(input);
        }

        private void UpdateAnimator()
        {
            bool grounded = _unitMotor.IsGrounded();
            _animator.SetFloat("Forward", _unitMotor.movementDirection.magnitude * (_unitMotor.isRunning ? 1 : 0.5f), 0.1f, Time.deltaTime);
            if (_animatorWings)
            {
                _animatorWings.SetFloat("Forward", _unitMotor.movementDirection.magnitude * (_unitMotor.isRunning ? 1 : 0.5f), 0.1f, Time.deltaTime);
                _animatorWings.SetBool("IsOpen", _unitMotor._isLevitating);
            }
            if (_animatorTail)
                _animatorTail.SetFloat("Forward", _unitMotor.movementDirection.magnitude * (_unitMotor.isRunning ? 1 : 0.5f), 0.1f, Time.deltaTime);
            if (_animatorHead)
                _animatorHead.SetFloat("Forward", _unitMotor.movementDirection.magnitude * (_unitMotor.isRunning ? 1 : 0.5f), 0.1f, Time.deltaTime);
            _animator.SetFloat("Turn", -Vector3.SignedAngle(_unitMotor.movementDirection, _unitMotor.transform.forward, Vector3.up) / 90f, 0.1f, Time.deltaTime);
            _animator.SetBool("Crouch", _unitMotor.crouch);
            _animator.SetBool("IsLevitation", _unitMotor._isLevitating);
            _animator.SetBool("Aiming", _unitMotor.isAiming);
            _animator.SetBool("OnGround", grounded || (_unitMotor.characterMovement.isOnGround && Vector3.Angle(-UnitMotor.gravity.normalized, UnitMotor.characterMovement.groundNormal) >= UnitMotor.characterMovement.slopeLimit && _groundedDelayTimer < groundedDelay));
            if (_bufferIsDie != _unitMotor.die)
            {
                _bufferIsDie = _unitMotor.die;
                _animator.SetTrigger(_bufferIsDie ? "Dead" : "Respawn");
            }

            Vector3 moveDir = transform.InverseTransformDirection(_unitMotor.movementDirection);
            //if (_unitMotor.characterMovement.velocity.onlyXZ().magnitude <= 0)
            //    moveDir *= 0;
            _animator.SetFloat("Horizontal", grounded ? moveDir.x : 0f, 0.1f, Time.deltaTime);
            _animator.SetFloat("Vertical", grounded ? moveDir.z : 0f, 0.1f, Time.deltaTime);
            if (!_unitMotor.characterMovement.isGrounded)
            {
                _animator.SetFloat("Jump", _unitMotor.characterMovement.velocity.y);
            }

            _multiAimLeftHandConstraint.weight = _unitMotor.isAiming ? 1f : 0f;
            _multiAimBody1Constraint.weight = _unitMotor.isAiming ? 1f : 0f;
            _multiAimBody2Constraint.weight = _unitMotor.isAiming ? 1f : 0f;
            _multiParentConstraint.weight = _unitMotor.isAiming ? 1f : 0f;
            //Debug.Log(_groundedDelayTimer);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            //_animator.SetLookAtPosition(_cameraController.transform.forward * 100 + _cameraController.transform.position);
            //if (_unitMotor.isAiming)
            //    _animator.SetLookAtWeight(_aimingWeight, _aimingBodyWeight, _aimingHeadWeight, 0f, _aimingClampWeight);
        }

        public void SetPlayer(Player player)
        {
            _player = player;
            //UpdateInput();
        }

        private void AnswerStartUsing(bool isUsing, IInteractable interactable)
        {
            if (_player != null && _player.isLocalPlayer)
                _unitMotor.SetMining(true);

            _animator.SetTrigger("StartMine");
            _isKeyboardMode = isUsing;
        }

        private void AnswerFinishUsing(bool isUsed, IInteractable interactable)
        {
            if (_player != null && _player.isLocalPlayer)
                _unitMotor.SetMining(false);

            _animator.SetTrigger("EndMine");
            _isKeyboardMode = false;
            if (isUsed && interactable is MiningResource resource)
            {
                InventoryScreenManager.Instance.AddStackableItem(resource.ID, 1);
            }
        }

        private List<IInteractable> _targetInteractables = new();
        private void CheckInteract()
        {
            if (_player.isLocalPlayer)
            {
                foreach (var target in _targetInteractables)
                {
                    if (target.IsCanInteract && !(target is MiningResource))
                    {
                        float newWeght = InventoryScreenManager.Instance.CurrentWeight + target.GetWeight();
                        if (target is MiningResource)
                            if (newWeght > _player.InventoryLimit)
                            {
                                Translator.Send(HUDProtocol.HelperInfo,
                                    new StringData { value = _targetInteractables[0].GetName() + "\n" + "Inventory full" });
                                continue;
                            }
                            //else
                            //    Translator.Send(HUDProtocol.HelperInfo, new StringData { value = "" });

                        target.Use(_player.gameObject, _player.AuthInfo, AnswerStartUsing, AnswerFinishUsing);
                        return;
                    }
                }
                //if (_targetInteractables.Count > 0)
                //    Translator.Send(HUDProtocol.HelperInfo,
                //        new StringData { value = _targetInteractables[0].GetName() + "\n" + "Inventory full" });
            }
        }

        private bool playMiningAnimation = true;
        public void UseTarget(IInteractable interactable, bool useMiningAnimaton = true)
        {
            playMiningAnimation = useMiningAnimaton;
            interactable.Use(_player.gameObject, _player.AuthInfo, AnswerStartUsing, AnswerFinishUsing);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_player.isLocalPlayer &&
                other.TryGetComponent(out IInteractable interactable) &&
                !_targetInteractables.Contains(interactable))
            {
                interactable.Hightlight(true);
                var bindings = _controls.Player.Interact.bindings;
                string nameBind = "";
                foreach (var binding in bindings)
                    if (binding.path.ToLower().Contains("keyboard"))
                        nameBind = binding.path.Split('/').Last();

                switch (interactable)
                {
                    case MiningResource resource:
                        Translator.Send(HUDProtocol.HelperInfo,
                            new StringData { value = interactable.GetName() + "\n" + "Use Multitool"});
                        break;
                    default:
                        Translator.Send(HUDProtocol.HelperInfo,
                            new StringData { value = interactable.GetName() + "\n" + "Press " + nameBind });
                        break;
                }

                _targetInteractables.Add(interactable);
            }
            if (_player.isLocalPlayer && other.CompareTag("Respawn"))
            {
                _isSafeble = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_player.isLocalPlayer &&
                other.TryGetComponent(out IInteractable interactable) &&
                _targetInteractables.Contains(interactable))
            {
                interactable.Hightlight(false);
                _targetInteractables.Remove(interactable);
                if (_targetInteractables.Count == 0)
                    Translator.Send(HUDProtocol.HelperInfo,
                        new StringData { value = "" });
            }
            if (_player.isLocalPlayer && other.CompareTag("Respawn"))
            {
                _isSafeble = false;
            }
        }
    }
}