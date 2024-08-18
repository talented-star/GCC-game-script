using GrabCoin.Helper;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GrabCoin.UI.Screens;

namespace GrabCoin.GameWorld.Player
{
  public class VRPlayerController : Interactor
  {
    [SerializeField] private float _speedWalk = 1;
    [SerializeField] private float _speedRun = 2;
    [SerializeField] private float _jumpStartYSpeedMperS = 8;
    [SerializeField] private int _startVrQuality = -1; // 0;
    [SerializeField] private ActionBasedContinuousMoveProvider _moveProvider;

    [SerializeField] private Transform _bodyTarget;
    [SerializeField] private Transform _leftHandTarget;
    [SerializeField] private Transform _rightHandTarget;
    [SerializeField] private Transform _headTarget;
    [SerializeField] private Transform _vrBody;
    [SerializeField] private Transform _vrLeftHand;
    [SerializeField] private Transform _vrRightHand;
    [SerializeField] private Transform _vrHead;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private CapsuleCollider _collider;
    [SerializeField] private XROrigin _xrOrigin;

    public static VRPlayerController Instance;

    private Transform _colliderTransform;
    private ColliderHelper _colliderHelper;

    private Controls _controls;
    private bool _flagRun = false;
    private bool _flagJump = false;

    private static int _currentQualityLevel = -1;

//    private bool IsGrounded => Physics.Raycast(_vrHead.position, Vector3.down, 2.0f);

    private float _avatarHeight;

    private float _jumpYSpeed;

    // private bool _isInteractableNear = false;
    private static bool _showVRHelp = true;

    private CharacterMoveHelper _charMoveHelper;
    private float _originalAvatarScale;

    private void Awake()
    {
      if (Instance != null)
      {
        Destroy(this);
        return;
      }
      Instance = this;

      _colliderTransform = _collider.transform;
      _colliderHelper = _colliderTransform.GetComponent<ColliderHelper>();

      _charMoveHelper = _xrOrigin.gameObject.GetComponent<CharacterMoveHelper>();

      _controls = new Controls();
      _controls.Player.SpeedChange.performed += ctx => ToggleRun();
      _controls.Player.IncreaseQuality.performed += ctx => IncreaseQualityLevel();
      _controls.Player.DecreaseQuality.performed += ctx => DecreaseQualityLevel();
      _controls.Player.Interact.performed += ctx => InteractWithInteractable();
      _controls.Player.Jump.performed += ctx => StartJump();

      if ((_startVrQuality != -1))
      {
        if (_currentQualityLevel == -1)
        {
          _currentQualityLevel = _startVrQuality;
        }
        QualitySettings.SetQualityLevel(_currentQualityLevel);
      }
    }

    private void Start()
    {
      _originalAvatarScale = _bodyTarget.localScale.y;
      // VRCalibrator.Instance?.CalibrateAvatar(_bodyTarget);
      VRCalibrator.Instance?.CalibrateAvatarCountingOriginalScale(_bodyTarget, _originalAvatarScale);

      // _isInteractableNear = false;
      if (_showVRHelp)
      {
        XR_UI.Instance.ShowScreen(XR_UI.Screen.Help);
        _showVRHelp = false;
      }
    }

    private void Update()
    {
      _avatarHeight = _xrOrigin.CameraInOriginSpaceHeight;

      Vector3 center = _xrOrigin.CameraInOriginSpacePos;
      _collider.center = new Vector3(center.x, _collider.center.y, center.z);
      _collider.height = _avatarHeight;

      if (_flagJump)
      {
        float t = Time.deltaTime;
        float dy = _jumpYSpeed * t;
        RaycastHit hitInfo;
        if ((_jumpYSpeed < 0) && Physics.Raycast(_vrHead.position, Vector3.down, out hitInfo, _avatarHeight + dy))
        {
          _flagJump = false;
          // Debug.Log("<=<=<=<=<= JUMP FINISHED");
        }
        else
        {
          _colliderTransform.position += Vector3.up * dy;
          _jumpYSpeed -= 9.81f * t;
        }
        // Debug.Log($"JUMP: {_flagJump}, {_colliderTransform.position.y}");
      }
    }

    public void SetAvatar (GameObject avatar)
    {
      GameObject goAvatar = Instantiate(avatar, transform);
      Transform tAvatar = goAvatar.transform;
      tAvatar.localPosition = Vector3.zero;
      tAvatar.localRotation = Quaternion.identity;
      VRAvatarController avatarController = goAvatar.GetComponentInChildren<VRAvatarController>();
      _bodyTarget = avatarController.transform;
      _headTarget = avatarController.Head.ikTarget;
      _leftHandTarget = avatarController.LeftHand.ikTarget;
      _rightHandTarget = avatarController.RightHand.ikTarget;
      avatarController.SetVRTargets(_vrHead, _vrLeftHand, _vrRightHand);
      _charMoveHelper.SetCharacter(avatarController);

      _originalAvatarScale = _bodyTarget.localScale.y;
      // VRCalibrator.Instance?.CalibrateAvatar(_bodyTarget);
      VRCalibrator.Instance?.CalibrateAvatarCountingOriginalScale(_bodyTarget, _originalAvatarScale);
    }

    public void OnQualityChanged()
    {
      _currentQualityLevel = QualitySettings.GetQualityLevel();
    }

    private void ToggleRun()
    {
      if (_isMoveDisabled) { return; }
      _flagRun = !_flagRun;
      _moveProvider.moveSpeed = _flagRun ? _speedRun : _speedWalk;

      // Debug.Log($"=>=>=>=>=> RUN: {_flagRun}");
    }

    public void IncreaseQualityLevel ()
    {
      QualitySettings.IncreaseLevel();
      OnQualityChanged();
    }

    public void DecreaseQualityLevel()
    {
      QualitySettings.DecreaseLevel();
      OnQualityChanged();
    }

    private void StartJump()
    {
      if (_isMoveDisabled) { return; }
      // Debug.Log($"=>=>=>=>=> JUMP: {_flagJump}");
      // Debug.Log($"JUMP: {_flagJump}, {_colliderTransform.position.y}");
      if (_flagJump) { return; }

      _flagJump = true;
      _jumpYSpeed = _jumpStartYSpeedMperS;
    }

    private void FinishJump ()
    {
      _rb.isKinematic = true;
      _flagJump = false;
    }

    private void OnEnable()
    {
      _controls.Enable();
      _colliderHelper.onTriggerEnter += Collider_OnTriggerEnter;
      _colliderHelper.onTriggerExit += Collider_OnTriggerExit;
    }

    private void OnDisable()
    {
      _controls.Disable();
      _colliderHelper.onTriggerEnter -= Collider_OnTriggerEnter;
      _colliderHelper.onTriggerExit -= Collider_OnTriggerExit;
    }

    public void Collider_OnTriggerEnter (Collider other)
    {
      Debug.Log($"VR Player triggered with {other.name}");

      other.TryGetComponent(out IInteractable interactable);
      if (interactable != null)
      {
        Interactable_OnTriggerEnter(interactable, _controls);
      }
      /*
      if (other.name.StartsWith("InteraciveConsole"))
      {
        // XR_UI.Instance.ShowHint("INTERACTIVE CONSOLE\nTo activate, press the GRIP button on your right controller");
        _isInteractableNear = true;
      }
      else if (other.name.StartsWith("KelvaResource"))
      {
        // XR_UI.Instance.ShowHint("KELVA RESOURCE\nTo harvest, press the GRIP button on your right controller");
        _isInteractableNear = true;
      }
      else if (other.name.StartsWith("LemmitResource"))
      {
        // XR_UI.Instance.ShowHint("LEMMIT RESOURCE\nTo harvest, press the GRIP button on your right controller");
        _isInteractableNear = true;
      }
      else if (other.name.StartsWith("PsionResource"))
      {
        // XR_UI.Instance.ShowHint("PSION RESOURCE\nTo harvest, press the GRIP button on your right controller");
        _isInteractableNear = true;
      }
      else if (other.name.StartsWith("RafidResource"))
      {
        // XR_UI.Instance.ShowHint("RAFID RESOURCE\nTo harvest, press the GRIP button on your right controller");
        _isInteractableNear = true;
      }
      */
    }

    public void Collider_OnTriggerExit (Collider other)
    {
      Debug.Log($"VR Player exit from triggering with {other.name}");
      // XR_UI.Instance.HideHint();
      // _isInteractableNear = false;

      other.TryGetComponent(out IInteractable interactable);
      if (interactable != null)
      {
        Interactable_OnTriggerExit(interactable);
      }
    }

    private void InteractWithInteractable ()
    {
      // if (!_isInteractableNear) { return; }

      CheckInteract();

      // XR_UI.Instance.ShowScreen(XR_UI.Screen.NotImplementedYet);
    }


    private float _locomotionSpeed = 0;
    private bool _isMoveDisabled = false;
    public void DisableMove ()
    {
      if (_isMoveDisabled) { return; }
      // _moveProvider.enabled = false;
      if (_moveProvider.moveSpeed > 0)
      {
        _locomotionSpeed = _moveProvider.moveSpeed;
      }
      _moveProvider.moveSpeed = 0;
      _isMoveDisabled = true;
    }

    public void EnableMove ()
    {
      if (!_isMoveDisabled) { return; }
      // _moveProvider.enabled = true;
      if (_locomotionSpeed > 0)
      {
        _moveProvider.moveSpeed = _locomotionSpeed;
      }
      _locomotionSpeed = 0;
      _isMoveDisabled = false;
    }
  }
}
