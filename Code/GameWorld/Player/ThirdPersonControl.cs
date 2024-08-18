using GrabCoin.GameWorld.Network;
using GrabCoin.Services.Chat;
using System;
using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Player
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonControl : MonoBehaviour
    {
        //public event Action ChatOpened;
        //public event Action ChatClosed;
        public event Action StartTalk;
        public event Action Talked;

        [SerializeField] private Transform _cam;
        private ThirdPersonCharacter _character; // A reference to the ThirdPersonCharacter on the object
        private Vector3 _camForward;             // The current forward direction of the camera
        private Vector3 _move;
        private bool _jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
        private bool _isKeyboardMode;

        private Controls _controls;

        [SerializeField] private Transform xr;

        [Inject] private PlayerState playerState;
        [SerializeField] private ChatPresenter _chatPresenter;

        private CustomEvent _onChatState;

        public bool IsCrouch => _controls.Player.Crouch.IsPressed();

        [Inject]
        private void Construct(Controls controls)
        {
            _controls = controls;
        }

        private void Start()
        {
            // get the third person character ( this should never be null due to require component )
            _character = GetComponent<ThirdPersonCharacter>();
            _onChatState = OnChatState;
            Translator.Add<UIPlayerProtocol>(_onChatState);
        }

        private void OnChatState(System.Enum codeEvent, ISendData data)
        {
            switch (codeEvent)
            {
                case UIPlayerProtocol.StateChat:
                    var state = (BoolData)data;
                    _isKeyboardMode = state.value;
                    break;
            }
        }

        private bool keyboardMode()
        {
            return _isKeyboardMode;// _chatPresenter?.IsOpened() ?? false;
        }
        private void Update()
        {
            if (keyboardMode())
            {
                return;
            }

            if (_controls.Player.PushToTalk.WasPressedThisFrame())
            {
                StartTalk?.Invoke();
            }

            if (_controls.Player.PushToTalk.IsPressed())
            {
                Talked?.Invoke();
            }

 
            if (!_jump)
            {
                _jump = _controls.Player.Jump.IsPressed();
            }

            if (_controls.Player.CallMenu.WasReleasedThisFrame())
            {
                //playerState.ActivateMenu();
            }

            if (_controls.Player.ChangeMode.WasReleasedThisFrame())
            {
                Enum.PlayerMode mode = playerState.PlayerMode == Enum.PlayerMode.ThirdPerson ? Enum.PlayerMode.VR : Enum.PlayerMode.ThirdPerson;
                playerState.ChangePlayerMode(mode);
            }
        }
        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            Vector2 moveVector = Vector2.zero;
            bool crouch = false;

            if (!keyboardMode())
            {
                moveVector = _controls.Player.Move.ReadValue<Vector2>();
                crouch = _controls.Player.Crouch.IsPressed();
            }
            
            float h = moveVector.x;
            float v = moveVector.y;

            Vector3 unitVector = new Vector3(1, 0, 1);

            // calculate move direction to pass to character
            //if (_cam != null)
            if (playerState.PlayerMode == Enum.PlayerMode.ThirdPerson)
            {
                // calculate camera relative direction to move:
                
                _camForward = Vector3.Scale(_cam.forward, unitVector).normalized;
                _move = v * _camForward + h * _cam.right;
            }
            //else
            //{
            //    // we use world-relative directions in the case of no main camera
            //    //_move = v * Vector3.forward + h * Vector3.right;
            //    _camForward = Vector3.Scale(xr.forward, unitVector).normalized;
            //    _move = v * _camForward + h * xr.right;
            //}
#if !MOBILE_INPUT
            // walk speed multiplier
            if (_controls.Player.SlowDown.IsPressed())
            {
                const float slowDownMultiplier = 0.5f;
                _move *= slowDownMultiplier;
            }
#endif

            // pass all parameters to the character control script
            _character.Move(_move, crouch, _jump);
            _jump = false;
        }
    }
}


