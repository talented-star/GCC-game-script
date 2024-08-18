using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyInputSettings
{
    public struct Data
    {
        public InputAction action;
        public string path;
        public string name;
        public string key;
        public int index;
    }

    private Controls _controls;
    //               controller           action     bind paths
    private Dictionary<string, Dictionary<string, List<Data>>> _bindings = new();
    private string[] res;

    public KeyInputSettings(Controls controls)
    {
        _controls = controls;
        PopulateData();
    }

    public string[] GetControlSchemes() =>
        _bindings.Keys.ToArray();

    public Dictionary<string, List<Data>> GetSchemesActionsData(string schemes) =>
        _bindings[schemes];

    public void PopulateData()
    {
        List<string> nameActions = new();
        List<string> bindingGroups = new();
        _bindings.Clear();
        foreach (var a in _controls.controlSchemes)
            _bindings.Add(a.bindingGroup, new());

        foreach (var action in _controls.bindings)
            if (!nameActions.Contains(action.action))
                nameActions.Add(action.action);


        foreach (var x in nameActions)
        {
            InputAction action = _controls.FindAction(x);
            SerealizeAction(action);
        }

        //foreach (var group in _bindings)
        //{
        //    Debug.Log($"==========={group.Key}===========");
        //    foreach (var action in group.Value)
        //    {
        //        Debug.Log($"__________{action.Key}__________");
        //        foreach (var bind in action.Value)
        //            Debug.Log($"{bind.action.name} {bind.name}||{bind.key}||{bind.index}||{bind.path}");
        //    }
        //}
    }

    private void SerealizeAction(InputAction actionToRebind)
    {
        if (actionToRebind == null)
            return;

        foreach (var y in actionToRebind.bindings)
        {
            if (y == null)
            {
                Debug.LogError("binding = null");
                continue;
            }
            if (y.groups == null)
            {
                Debug.LogWarning($"{y.path}: binding group = null");
                continue;
            }
            if (!_bindings.ContainsKey(y.groups))
                _bindings[y.groups] = new Dictionary<string, List<Data>>();
            if (_bindings[y.groups].ContainsKey(actionToRebind.name))
            {
                if (_bindings[y.groups][actionToRebind.name] == null)
                    _bindings[y.groups][actionToRebind.name] = new List<Data>();

                res = Split(y.hasOverrides && !string.IsNullOrWhiteSpace(y.overridePath) ? y.overridePath : y.path);
                res = res[1..res.Length];
                int bindIndex = actionToRebind.GetBindingIndex(y);
                Data data = new Data
                {
                    action = actionToRebind,
                    path = y.path,
                    name = y.isPartOfComposite ? y.name : "",
                    key = string.Join(' ', res),
                    index = bindIndex,
                };
                _bindings[y.groups][actionToRebind.name].Add(data);
                LoadBindingOverride(data);
            }
            else
            {
                res = Split(y.hasOverrides && !string.IsNullOrWhiteSpace(y.overridePath) ? y.overridePath : y.path);
                res = res[1..res.Length];
                int bindIndex = actionToRebind.GetBindingIndex(y);
                Data data = new Data
                {
                    action = actionToRebind,
                    path = y.path,
                    name = y.isPartOfComposite ? y.name : "",
                    key = string.Join(' ', res),
                    index = bindIndex,
                };
                _bindings[y.groups].Add(actionToRebind.name, new List<Data> { data });
                LoadBindingOverride(data);
            }
        }
    }

    public void LoadBindingOverride(Data data)
    {
        if (PlayerPrefs.HasKey(GetSaveKey(data)))
        {
            string rebind = PlayerPrefs.GetString(GetSaveKey(data), string.Empty);
            if (string.IsNullOrEmpty(rebind)) return;
            data.action.LoadBindingOverridesFromJson(rebind);
        }
    }

    private string GetSaveKey(Data data) =>
        data.action.actionMap + data.action.name + data.index;

    private string[] Split(string input)
    {
        RegexOptions options = RegexOptions.None;
        Regex regex = new Regex(@"\w+", options);
        return regex.Matches(input).Select(match => match.Value).ToArray();
    }
}
