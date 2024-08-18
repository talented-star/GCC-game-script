using UnityEngine;
using GrabCoin.GameWorld.Player;
using GrabCoin.Model;
using System;
using Zenject;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;
using GrabCoin.UI;
using InventoryPlus;

namespace GrabCoin.GameWorld
{
    public class LaboratoryConsole : MonoBehaviour, IInteractable
    {
        [SerializeField] private QuickOutline.Outline _outline;

        private UIScreensManager _screensManager;

        public bool IsCanInteract => true;

        public string Name => "Laboratory console";

        public string GetName() =>
            Name;

        public float GetWeight() => 0;

        [Inject]
        private void Construct(UIScreensManager screensManager)
        {
            _screensManager = screensManager;
        }

        public void Hightlight(bool isActive)
        {
            _outline.enabled = isActive;
        }

        public async void Use(GameObject netIdentity, AuthInfo authInfo, Action<bool, IInteractable> answerStartUsing, Action<bool, IInteractable> answerFinishUsing)
        {
            await _screensManager.Open<LaboratoryScreen>();
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

    }
}
