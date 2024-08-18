using UnityEngine;
using GrabCoin.GameWorld.Player;
using GrabCoin.Model;
using System;
using GrabCoin.Services.Backend.Catalog;
using Zenject;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;
using TMPro;

namespace GrabCoin.GameWorld
{
    public class RafineryConsole : MonoBehaviour, IInteractable
    {
        public enum TypeConsole
        {
            Refinery,
            Laboratory,
            Workshop,
            Storage
        }
        [SerializeField] private QuickOutline.Outline _outline;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TypeConsole _typeConsole;

        private PlayerScreensManager _screensManager;

        public bool IsCanInteract => true;

        public string Name => $"{_typeConsole} console";

        public string GetName() =>
            Name;

        public float GetWeight() => 0;

        [Inject]
        private void Construct(PlayerScreensManager screensManager)
        {
            _screensManager = screensManager;
            _nameText.text = _typeConsole.ToString();
        }

        public void Hightlight(bool isActive)
        {
            _outline.enabled = isActive;
        }

        public async void Use(GameObject netIdentity, AuthInfo authInfo, Action<bool, IInteractable> answerStartUsing, Action<bool, IInteractable> answerFinishUsing)
        {
            switch (_typeConsole)
            {
                case TypeConsole.Refinery: await _screensManager.OpenScreen<RafineryScreen>(); break;
                case TypeConsole.Laboratory: await _screensManager.OpenScreen<LaboratoryScreen>(); break;
                case TypeConsole.Workshop: await _screensManager.OpenScreen<WorkshopScreen>(); break;
                case TypeConsole.Storage: await _screensManager.OpenScreen<BankScreen>(); break;
            }
            //Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }
    }
}
