using GrabCoin.Services.Chat.VoiceChat;
using UnityEngine;
using Zenject;
using UnityEngine.XR;
using Unity.XR;
using Unity.XR.CoreUtils;

namespace GrabCoin.GameWorld.Player
{
    public class FirstPersonCharacterController : MonoBehaviour
    {
        [SerializeField] private float turnSpeed;
        [SerializeField] private Transform character;
        [SerializeField] private ThirdPersonControl thirdPersonControl;
        [SerializeField] private VoiceHudView voiceHudView;
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private float standHeight;
        [SerializeField] private float crouchHeight;
        [SerializeField] private float standCrouchSwitchSpeed;
        [field: SerializeField] public Transform CameraVR { get; private set; }

        [Inject] private Controls controls;

        private Vector2 turnVector;
        private Vector3 rotation;

        private void Start()
        {
            thirdPersonControl.StartTalk += OnStartTalk;
            thirdPersonControl.Talked += OnTalked;
        }

        private void OnDestroy()
        {
            thirdPersonControl.StartTalk -= OnStartTalk;
            thirdPersonControl.Talked -= OnTalked;
        }

        private void Update()
        {
            turnVector = controls.Player.MoveCamera.ReadValue<Vector2>() * turnSpeed;
            rotation = new Vector3(0f, turnVector.x, 0f);
            transform.Rotate(rotation * turnSpeed * Time.deltaTime);
            transform.position = character.position;
            xrOrigin.CameraYOffset = Mathf.MoveTowards(xrOrigin.CameraYOffset, thirdPersonControl.IsCrouch ? crouchHeight : standHeight, standCrouchSwitchSpeed * Time.deltaTime);
        }

        private void OnStartTalk()
        {
            voiceHudView.OnPlaying();
        }

        private void OnTalked()
        {
            voiceHudView.OnPlaying();
        }
    }
}
