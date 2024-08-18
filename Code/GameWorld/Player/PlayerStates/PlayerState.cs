using System;
using GrabCoin.Enum;
using GrabCoin.UI;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Player
{
    public class PlayerState
    {
        public event Action PlayerModeChangedEvent = delegate { };
        public event Action MenuActiveEvent = delegate { };

        private UIScreensManager _screensManager;

        public PlayerMode PlayerMode { get; private set; } = PlayerMode.ThirdPerson;

        public bool IsMenuActive { get; private set; }

        public string PlayFabID { get; set; }
        public bool raidWithGC;

        [Inject]
        private void Construct(UIScreensManager screensManager)
        {
            _screensManager = screensManager;
        }

        public void ChangePlayerMode(PlayerMode mode)
        {
            if (PlayerMode != mode)
            {
                PlayerMode = mode;
                PlayerModeChangedEvent.Invoke();
            }
        }

        public async void ActivateMenu(UIScreensManager screensManager)
        {
            if (ScreenOverlayManager.GetActiveWindow() && !IsMenuActive)
            {
                return;
            }
            if (IsMenuActive) return;
            IsMenuActive = true;
            //Cursor.lockState = IsMenuActive ? CursorLockMode.None : CursorLockMode.Locked;
            //Cursor.visible = IsMenuActive;
            var screen = await screensManager.Open<InGameMenu>();
            await screen.Process();
            IsMenuActive = false;
            //Cursor.lockState = IsMenuActive ? CursorLockMode.None : CursorLockMode.Locked;
            //Cursor.visible = IsMenuActive;
        }
    }
}
