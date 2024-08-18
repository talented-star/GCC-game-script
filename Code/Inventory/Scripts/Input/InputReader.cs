using GrabCoin.UI.HUD;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

namespace InventoryPlus
{
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(StandaloneInputModule))]
    public class InputReader : MonoBehaviour
    {
        public enum ActionState { Inventory, HUD, Both };

        [Header("Inputs")]
        public string InventoryOnHorizontalInput = "Horizontal";
        public string InventoryOffHorizontalInput = "Mouse ScrollWheel";
        public bool enableMouseInventoryOff = false;

        [Space(15)]
        [Header("Actions")]
        public ActionState performUse = ActionState.Both;
        public ActionState performSort = ActionState.Both;
        public ActionState performSwap = ActionState.Both;
        public ActionState performEquip = ActionState.Both;

        [Space(15)]
        [Header("Audio")]
        public bool playAudioOnSelection = false;
        public AudioSource selectionAudio;

        [Space(15)]
        [Header("References")]
        public Inventory inventory;
        public UIDetails details;
        public InputAction inputAction;
        public bool autoInit = false;


        public bool inventoryOn = false;
        private int currentSelectedHotbarSlot = 0;

        private GameObject currentSelectedObj = null;
        private StandaloneInputModule inputModule;
        private EventSystem eventSystem;
        private PlayerScreensManager _screensManager;

        public UnityAction<int> OnHotbarSlotSelected;

        /**/

        #region Setup

        private void Awake()
        {
            //if(autoInit)
            //    Init();
        }

        public void Init(PlayerScreensManager screensManager)
        {
            _screensManager = screensManager;
            inputModule = this.GetComponent<StandaloneInputModule>();
            eventSystem = this.GetComponent<EventSystem>();

            //set initial state
            inputModule.horizontalAxis = InventoryOffHorizontalInput;
            inventory.SelectFirstHotbarSlot();
            inventory.ClearSwap();

            inventory.OnSort += delegate { SelectCurrentHotbarSlot(); };
            inventory.OnSwap += delegate { SelectCurrentHotbarSlot(); };

            //inputAction.started += ToggleInventory;
            inventoryOn = true;
            SelectCurrentHotbarSlot();
        }

        private void OnDisable()
        {
            inventory.OnSort -= delegate { SelectCurrentHotbarSlot(); };
            inventory.OnSwap -= delegate { SelectCurrentHotbarSlot(); };
        }

        #endregion


        #region Inputs

        private void Update()
        {
            //if (ScreenOverlayManager.GetActiveWindow() != null && ScreenOverlayManager.GetActiveWindow() != InventoryScreenManager.Instance)
            //{
            //    return;
            //}
            UpdateSelection();

            InventoryActions();
        }


        public void UpdateSelection()
        {
            GameObject tmpObj = currentSelectedObj;

            //handle selection when no GameObject are selected
            if (eventSystem.currentSelectedGameObject != null) currentSelectedObj = eventSystem.currentSelectedGameObject;
            else eventSystem.SetSelectedGameObject(currentSelectedObj);

            //handle selection change
            if (tmpObj != currentSelectedObj)
            {
                if (playAudioOnSelection) selectionAudio.Play();
                if (details != null && inventoryOn) details.UpdateDetails(currentSelectedObj.GetComponent<UISlot>(), true);
            }
        }


        private void ToggleInventory(CallbackContext callback)
        {
            if (!inventoryOn && !PlayerScreensManager.Instance.CanBeOpened(InventoryScreenManager.Instance))
                return;

            inventoryOn = !inventoryOn;
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = inventoryOn });

            ShowCursor(inventoryOn);
            //DISABLE CONTROLLER HERE
            inventory.ShowInventory(inventoryOn);
            inventory.ForceEndSwap();

            //inventory open - inventory closed
            if (inventoryOn)
            {
                inputModule.horizontalAxis = InventoryOnHorizontalInput;
                inventory.SelectFirstInventorySlot();
                //PlayerScreensManager.Instance.OpenScreen(InventoryScreenManager.Instance);


            }
            else
            {
                inputModule.horizontalAxis = InventoryOffHorizontalInput;
                inventory.SelectHotbarSlot(currentSelectedHotbarSlot);
                //PlayerScreensManager.Instance.OpenScreen(null);
            }
        }


        private void InventoryActions()
        {
            ////use action
            //if (Input.GetKeyDown(KeyCode.U) && currentSelectedObj != null && ((performUse == ActionState.Inventory && inventoryOn) || (performUse == ActionState.HUD && !inventoryOn) || (performUse == ActionState.Both)))
            //{
            //    InventoryScreenManager.Instance.UseItem(currentSelectedObj.GetComponent<UISlot>());
            //    if (details != null && inventoryOn) details.UpdateDetails(currentSelectedObj.GetComponent<UISlot>(), false);
            //}

            //////drop action
            ////if (Input.GetKeyDown(KeyCode.Q) && currentSelectedObj != null && ((performDrop == ActionState.Inventory && inventoryOn) || (performDrop == ActionState.HUD && !inventoryOn) || (performDrop == ActionState.Both)))
            ////{
            ////    inventory.DropItem(currentSelectedObj.GetComponent<UISlot>());
            ////    if (details != null && inventoryOn) details.UpdateDetails(currentSelectedObj.GetComponent<UISlot>(), false);
            ////}

            ////sort action
            //if (Input.GetKeyDown(KeyCode.M) && currentSelectedObj != null && ((performSort == ActionState.Inventory && inventoryOn) || (performSort == ActionState.HUD && !inventoryOn) || (performSort == ActionState.Both)))
            //{
            //    inventory.Sort();
            //    if (details != null && inventoryOn) details.UpdateDetails(currentSelectedObj.GetComponent<UISlot>(), false);
            //}

            ////equip action
            //if (Input.GetKeyDown(KeyCode.E) && currentSelectedObj != null && ((performEquip == ActionState.Inventory && inventoryOn) || (performEquip == ActionState.HUD && !inventoryOn) || (performEquip == ActionState.Both)))
            //{
            //    inventory.EquipItem(currentSelectedObj.GetComponent<UISlot>());
            //    if (details != null && inventoryOn) details.UpdateDetails(currentSelectedObj.GetComponent<UISlot>(), false);
            //}

            ////swap action - clear swap
            //if (Input.GetKeyDown(KeyCode.N) && currentSelectedObj != null && ((performSwap == ActionState.Inventory && inventoryOn) || (performSwap == ActionState.HUD && !inventoryOn) || (performSwap == ActionState.Both)))
            //{
            //    inventory.SwapItem(currentSelectedObj.GetComponent<UISlot>());
            //    if (details != null && inventoryOn) details.UpdateDetails(currentSelectedObj.GetComponent<UISlot>(), false);
            //}
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    inventory.ClearSwap();
            //    if (details != null && inventoryOn) details.UpdateDetails(currentSelectedObj.GetComponent<UISlot>(), false);
            //}
            if (Input.inputString.Length > 0 && int.TryParse(Input.inputString, out int result)&&!details.gameObject.activeInHierarchy)
            {
                SelectCurrentHotbarSlot(result - 1);
            }
        }

        public void SelectCurrentHotbarSlot(int index = -1) 
        {
            if (_screensManager && !_screensManager.EqualsCurrentScreen<GameHud>()) return;
            if (inventory.SelectHotbarSlot(index == -1 ? currentSelectedHotbarSlot : index)) 
            {
                currentSelectedHotbarSlot = index == -1 ? currentSelectedHotbarSlot : index;
                OnHotbarSlotSelected?.Invoke(index == -1 ? currentSelectedHotbarSlot : index);
            }
        }

        #endregion


        #region Utils

        private void ShowCursor(bool _show)
        {
            if (!enableMouseInventoryOff)
            {
                if (_show)
                {
                    //Cursor.lockState = CursorLockMode.None;
                    //Cursor.visible = true;
                }
                else
                {
                    //Cursor.lockState = CursorLockMode.Locked;
                    //Cursor.visible = false;
                }
                //Debug.Log($"<color=blue>Cursor visible: {Cursor.visible}</color>");
            }
        }

        #endregion
    }
}