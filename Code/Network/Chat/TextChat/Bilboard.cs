using GrabCoin.GameWorld.Player;
using UnityEngine;
using Zenject;

public class Bilboard : MonoBehaviour
{
    private static ThirdPersonCameraController _personCamera;
    private static Transform _vrCamera;

    [Inject] private PlayerState _playerState;

    private void Start()
    {
        if(_personCamera == null )
        _personCamera = FindObjectOfType<ThirdPersonCameraController>();

        if (_vrCamera == null)
            _vrCamera = FindAnyObjectByType<FirstPersonCharacterController>(FindObjectsInactive.Include)?.CameraVR ?? null;
    }

    private void Update()
    {
        if (_playerState.PlayerMode == GrabCoin.Enum.PlayerMode.ThirdPerson)
        {
            LookToThirdPersonCamera();
        }
        else if (_playerState.PlayerMode == GrabCoin.Enum.PlayerMode.VR)
        {
            LookToFirstPersonCamera();
        }
    }

    private void LookToThirdPersonCamera()
    {
        if (_personCamera == null)
        {
            Start();
            return;
        }
        transform.rotation = _personCamera.transform.rotation;
    }

    private void LookToFirstPersonCamera()
    {
        if (_vrCamera == null)
        {
            Start();
            return;
        }
        transform.rotation = _vrCamera.rotation;
    }
}
