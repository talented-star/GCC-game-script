using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace GrabCoin.GameWorld.Player
{
    public class CommonPlayer : MonoBehaviour
    {
        [SerializeField] private Transform playerVR;
        [SerializeField] private Transform playerThird;
        [SerializeField] private SkinnedMeshRenderer[] renderers;
        [SerializeField] private GameObject cameraThird;

        [Inject]
        private PlayerState playerState;

        private void Start()
        {
            playerState.PlayerModeChangedEvent += OnPlayerModeChanged;
            OnPlayerModeChanged();
        }

        private void OnDestroy()
        {
            playerState.PlayerModeChangedEvent -= OnPlayerModeChanged;
        }

        private void OnPlayerModeChanged()
        {
            if (playerVR == null)
            {
                return;
            }

            playerVR.gameObject.SetActive(playerState.PlayerMode == Enum.PlayerMode.VR);
            cameraThird.gameObject.SetActive(playerState.PlayerMode == Enum.PlayerMode.ThirdPerson);

            SetupCharacterVisability();
        }


        private void SetupCharacterVisability()
        {
            ShadowCastingMode mode = playerState.PlayerMode == Enum.PlayerMode.VR ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            foreach (var item in renderers)
            {
                item.shadowCastingMode = mode;
            }
        }
    }
}
