using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GrabCoin.UI.Screens
{
    public class KeyBindSlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameAction;
        [SerializeField] private TMP_Text _namekey;
        [SerializeField] private Button _rebindButton;
        [SerializeField] private Button _resetButton;

        private KeyInputSettings.Data _data;

        public event Action rebindComplete;
        public event Action rebindCanceled;
        public event Action<InputAction, int> rebindStarted;
        public event Action reseted;

        public void Populate(KeyInputSettings.Data data)
        {
            _data = data;
            var bind = _data.action.bindings[_data.index];
            var res = Split(bind.effectivePath);
            res = res[1..res.Length];
            var key = string.Join(' ', res);

            _nameAction.text = $"{_data.action.name} {bind.name}";
            _namekey.text = key;
            _rebindButton.onClick.AddListener(DoRebind);
            _resetButton.onClick.AddListener(ResetBinding);
        }

        private string GetSaveKey() =>
            _data.action.actionMap + _data.action.name + _data.index;


        private void DoRebind()
        {
            if (_data.action == null || _data.index < 0)
                return;

            _namekey.text = "Press new key";
            _rebindButton.interactable = false;
            _data.action.Disable();

            var rebind = _data.action.PerformInteractiveRebinding(_data.index)
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(async operation =>
                {
                    operation.Dispose();

                    SaveBindingOverride();
                    await UniTask.DelayFrame(5);
                    Filling();
                    _rebindButton.interactable = true;
                    _data.action.Enable();
                })
                .OnCancel(async operation =>
                {
                    _data.action.Enable();
                    operation.Dispose();
                    await UniTask.DelayFrame(5);
                    Filling();
                })
                .Start();

            rebindStarted?.Invoke(_data.action, _data.index);
        }

        private void SaveBindingOverride()
        {
            string rebinds = _data.action.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(GetSaveKey(), rebinds);
        }

        public async void ResetBinding()
        {
            _data.action.RemoveBindingOverride(_data.index);

            PlayerPrefs.DeleteKey(GetSaveKey());
            await UniTask.DelayFrame(5);
            Filling();
        }

        private void Filling()
        {
            var bind = _data.action.bindings[_data.index];
            var res = Split(bind.hasOverrides && !string.IsNullOrWhiteSpace(bind.overridePath) ? bind.overridePath : bind.path);
            res = res[1..res.Length];
            _data.key = string.Join(' ', res);
            _namekey.text = _data.key;
        }

        private string[] Split(string input)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"\w+", options);
            return regex.Matches(input).Select(match => match.Value).ToArray();
        }
    }
}
