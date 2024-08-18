using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using VRCore.Config;
using static UnityEngine.XR.Interaction.Toolkit.DeviceBasedContinuousMoveProvider;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace VRCore.Input
{
    public class InputSystemHelper : MonoBehaviour
    {
        [SerializeField] private XRBaseController _controller;
        private InputActionManager _inputActionManager;

        private InputAction _menuAction;
        private InputAction _debugLCM;

        public event Action<InputAction.CallbackContext> onMenuButtonDown;
        public event Action<InputAction.CallbackContext> onMenuButtonUp;

        public event Action<InputAction.CallbackContext> ondebugLCMDown;
        public event Action<InputAction.CallbackContext> ondebugLCMUp;

        static readonly InputFeatureUsage<Vector2>[] k_Vec2UsageList =
        {
            CommonUsages.primary2DAxis,
            CommonUsages.secondary2DAxis,
        };
        InputAxes m_InputBinding = InputAxes.Primary2DAxis;

        void Awake()
        {
            _inputActionManager = GetComponent<InputActionManager>();

            //menuAction = inputActionManager.actionAssets.First().FindActionMap(InputConstants.LEFT_HAND_INTERACTION_MAP).FindAction(InputConstants.MENU_ACTION);

            //menuAction.started += OnMenuButtonDown;
            //menuAction.canceled += OnMenuButtonUp;
            var map = _inputActionManager.actionAssets.First().FindActionMap(InputConstants.LEFT_HAND_INTERACTION_MAP);
            _debugLCM = map.FindAction(InputConstants.TRIGGER_ACTION);
            _debugLCM.canceled += OnDebugLCMUp;


            //var feature = k_Vec2UsageList[(int)m_InputBinding];
            //if (controller != null &&
            //        controller.enableInputActions &&
            //        controller.inputDevice.TryGetFeatureValue(feature, out var controllerInput))
            //{
            //    //input += GetDeadzoneAdjustedValue(controllerInput);
            //}
        }

        #region "Reaction"
        private void OnMenuButtonUp(InputAction.CallbackContext obj)
        {
            onMenuButtonUp?.Invoke(obj);
        }

        private void OnMenuButtonDown(InputAction.CallbackContext obj)
        {
            onMenuButtonDown?.Invoke(obj);
        }

        private void OnDebugLCMUp(InputAction.CallbackContext obj)
        {
            ondebugLCMUp?.Invoke(obj);
        }
        #endregion "Reaction"
    }
}