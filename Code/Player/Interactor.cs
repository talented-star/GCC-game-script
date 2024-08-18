using GrabCoin.GameWorld.Resources;
using GrabCoin.UI.Screens;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GrabCoin.GameWorld.Player
{
  public class Interactor : MonoBehaviour
  {
    private List<IInteractable> _targetInteractables = new();

    private Player _player;

    private bool _isVR = false;

    public void SetPlayer(Player player, bool isVR)
    {
      _isVR = isVR;
      _player = player;
    }

    private void AnswerStartUsing(bool isUsing, IInteractable interactable)
    {
      /*
      if (_player != null && _player.isLocalPlayer)
        _unitMotor.SetMining(true);

      _animator.SetTrigger("StartMine");
      _isKeyboardMode = isUsing;
      */
    }

    private void AnswerFinishUsing(bool isUsed, IInteractable interactable)
    {
      /*
      if (_player != null && _player.isLocalPlayer)
        if (!_isVR)
        _unitMotor.SetMining(false);

      _animator.SetTrigger("EndMine");
      _isKeyboardMode = false;
      */
      if (isUsed && interactable is MiningResource resource)
      {
        InventoryScreenManager.Instance.AddStackableItem(resource.ID, 1);
        //InventoryScreenManager.Instance.InventoryDataManager.RefreshInventory();
      }
    }

    private enum VRAction { Use = 0, Press = 1, Click = 22, Touch = 3, Move = 4 }
    private string[] VRActionSeekName = { null, "press", "click", "touch", "move" };
    private string[] VRActionHintName = { "Use", "Press", "Click", "Touch", "Move" };
    private enum VRHand { Left = 0, Right = 1 }
    private string[] VRHandSeekName = { "left", "right" };
    private string[] VRHandHintName = { "Left", "Right" };
    private enum VRButton { Grip = 0, Trigger = 1, PrimaryButton = 2, SecondaryButton = 3, Joystick = 4, MenuButton = 5, Undefined = -1 }
    private string[] VRButtonSeekName = { "grip", "trigger", "primarybutton", "secondarybutton", "primary2daxis", "menu" }; // TODO: Check real names
    private string[] VRButtonHintName = { "Grip", "Trigger", "Primary Button", "Secondary Button", "Joystick", "Menu Button" };

    private string BindingName (Controls controls, bool isVR)
    {
      var bindings = controls.Player.Interact.bindings;
      string nameBind = "";
      foreach (var binding in bindings)
      {
        Debug.Log($"Binding: \"{binding.path}\"");

        if (_isVR)
        {
          string s = binding.path.ToLower();
          Debug.Log($"s = \"{s}\"");
          VRHand hand = VRHand.Right;
          VRAction action = VRAction.Use;
          VRButton button = VRButton.Undefined;

          if (s.Contains("<xrcontroller>"))
          {
            for (int i = 0; i < VRButtonSeekName.Length; i++)
            {
              if (s.Contains(VRButtonSeekName[i]))
              {
                button = (VRButton)i;
              }
            }
            if (button != VRButton.Undefined)
            {
              for (int i = 0; i < VRHandSeekName.Length; i++)
              {
                if (s.Contains(VRHandSeekName[i]))
                {
                  hand = (VRHand)i;
                }
              }
              for (int i = 1; i < VRActionSeekName.Length; i++)
              {
                if (s.Contains(VRActionSeekName[i]))
                {
                  action = (VRAction)i;
                }
              }
              return $"{VRActionHintName[(int)action]} {VRHandHintName[(int)hand]} {VRButtonHintName[(int)button]}";
            }
          }
        }
        else
        {
          if (binding.path.ToLower().Contains("keyboard"))
            nameBind = binding.path.Split('/').Last();
        }
      }
      return nameBind;
    }

    protected void Interactable_OnTriggerEnter (IInteractable interactable, Controls controls)
    {
      if (!_targetInteractables.Contains(interactable))
      {
        interactable.Hightlight(true);

        string nameBind = BindingName(controls, _isVR);

        string msg = interactable.GetName() + "\n" + (_isVR ? nameBind : ("Press " + nameBind));
        if (_isVR)
        {
          XR_UI.Instance.ShowHint(msg);
        }
        else
        {
          Translator.Send(HUDProtocol.HelperInfo,
              new StringData { value = msg });
        }
        _targetInteractables.Add(interactable);
      }
    }

    protected void Interactable_OnTriggerExit (IInteractable interactable)
    {
      if (_targetInteractables.Contains(interactable))
      {
        interactable.Hightlight(false);
        _targetInteractables.Remove(interactable);
        if (_targetInteractables.Count == 0)
        {
          if (_isVR)
          {
            XR_UI.Instance.HideHint();
          }
          else
          {
            Translator.Send(HUDProtocol.HelperInfo,
                new StringData { value = "" });
          }
        }
      }
    }


    protected void CheckInteract ()
    {
      Debug.Log("Interactor.CheckInteract() START");
      if (_player.isLocalPlayer)
      {
        foreach (var target in _targetInteractables)
        {
          if (target.IsCanInteract)
          {
            float newWeght = InventoryScreenManager.Instance.CurrentWeight + target.GetWeight();
            if (target is MiningResource)
              if (newWeght > _player.InventoryLimit)
                continue;
            Debug.Log("Interactor.CheckInteract() USE and RETURN");
            target.Use(_player.gameObject, _player.AuthInfo, AnswerStartUsing, AnswerFinishUsing);
            if (_isVR)
            {
              XR_UI.Instance.HideHint();
            }
            return;
          }
        }
        if (_targetInteractables.Count > 0)
        {
          string msg = _targetInteractables[0].GetName() + "\n" + "Inventory full";
          if (_isVR)
          {
            XR_UI.Instance.ShowHint(msg);
          }
          else
          {
            Translator.Send(HUDProtocol.HelperInfo,
                new StringData { value = msg });
          }
        }
      }
      Debug.Log("Interactor.CheckInteract() FINISH");
    }
  }
}
