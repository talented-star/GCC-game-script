using UnityEngine;
using UnityEngine.UI;
using Zenject;
using GrabCoin.Enum;
using GrabCoin.GameWorld.Player;

namespace GrabCoin.UI
{
    public class ChangePlayerModeButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private PlayerMode playerMode;

        [Inject] private PlayerState playerState;

        private void Start()
        {
            //button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            //button.onClick.RemoveAllListeners();
        }

        private void OnClick()
        {
            //playerState.ChangePlayerMode(playerMode);
        }
    }
}

