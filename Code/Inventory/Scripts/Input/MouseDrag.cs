using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace InventoryPlus
{
    [RequireComponent(typeof(UISlot))]
    public class MouseDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private UISlot UISlot;
        private Inventory inventory;
        private GameObject dragInstance;


        /**/


        #region Setup

        public void SetInventory(UISlot _UISlot, Inventory _inventory)
        {
            UISlot = _UISlot;
            inventory = _inventory;
        }

        #endregion


        #region DragEvents

        public void OnBeginDrag(PointerEventData eventData)
        {
            var itemIndex = inventory.GetItemIndex(UISlot);

            if (!inventory.HasItemSlot(itemIndex))
                return;

            var slot = inventory.GetItemSlot(itemIndex);

            if (slot.GetItemType() == null)
                return;

            inventory.SwapItem(UISlot);

            //instanciate dragged object preview
            if(UISlot.GetIsShown())
            {
                dragInstance = new GameObject();
                dragInstance.name = "Drag Image: " + UISlot.name;

                dragInstance.AddComponent<Image>();
                dragInstance.GetComponent<Image>().sprite = UISlot.itemImg.sprite;

                dragInstance.transform.SetParent(inventory.pickupContext);
                dragInstance.transform.localScale = Vector3.one;

                dragInstance.GetComponent<Image>().raycastTarget = false;
            }
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (dragInstance != null) dragInstance.transform.position = Input.mousePosition;
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject targetObj = eventData.pointerCurrentRaycast.gameObject;

            //trigger swap if possible
            if (targetObj != null && targetObj.TryGetComponent(out UISlot slot))
            {
                inventory.SwapItem(slot);
                GameObject.Destroy(dragInstance);
            }
            else
            {
                inventory.ClearSwap();
                UISlot.SetSwapState(false);
                GameObject.Destroy(dragInstance);
            }

            EventSystem.current.SetSelectedGameObject(targetObj);
        }


        public void ForceEndMouseDrag()
        {
            inventory.ClearSwap();
            UISlot.SetSwapState(false);
            GameObject.Destroy(dragInstance);
        }

        #endregion
    }
}