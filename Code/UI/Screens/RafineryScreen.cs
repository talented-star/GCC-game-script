using UnityEngine;
using UnityEngine.UI;
using Zenject;
using GrabCoin.UI.ScreenManager;
using System.Collections.Generic;
using PlayFab.ClientModels;
using System;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using NaughtyAttributes;
using System.Linq;
using TMPro;
using System.Threading.Tasks;
using GrabCoin.UI.HUD;
using Cysharp.Threading.Tasks;
using static UnityEngine.Application;

namespace GrabCoin.UI.Screens
{
  [Serializable]
  public class RafineryData
  {
    public string itemId;
    public int count;
    public DateTime startRafinering;
  }

  [UIScreen("UI/Screens/RafineryScreen.prefab")]
  public class RafineryScreen : UIScreenBase
  {
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _totalCountText;
    [SerializeField] private TMP_Text _walletText;
    [SerializeField] private Transform _resourcesContext;
    [SerializeField] private Transform _queueContext;
    [SerializeField] private ItemRafinerySlot _itemSlot;
    [SerializeField] private QueueRafinerySlot _queueSlot;
    [HorizontalLine(color: EColor.Gray)]
    [SerializeField] private int _countSlotSQueue;

    //private bool _allSlotsBusy;
    private List<QueueRafinerySlot> _rafinerySlots = new();
    private List<ItemRafinerySlot> _itemSlots = new();
    private List<RafineryData> _rafineryDatas = new();
    private WaitAnswerServerScreen _waitAnswer;

    private CatalogManager _catalogManager;
    private InventoryDataManager _inventoryManager;
    //private UIPopupsManager _popupsManager;
    private PlayerScreensManager _screensManager;

    [Inject]
    public void Construct(
        CatalogManager catalogManager,
        InventoryDataManager inventoryManager,
        //UIPopupsManager popupsManager,
        PlayerScreensManager screensManager
        )
    {
      _catalogManager = catalogManager;
      _inventoryManager = inventoryManager;
      //_popupsManager = popupsManager;
      _screensManager = screensManager;
    }

    private void Awake()
    {
      _backButton.onClick.AddListener(Back);

      for (int i = 0; i < _countSlotSQueue; i++)
      {
        var slot = Instantiate(_queueSlot, _queueContext);
        slot.Populate();
        slot.onGetCurrency = GetRafineryCurrency;
        _rafinerySlots.Add(slot);
      }
    }

    private void OnEnable()
    {
      _walletText.text = _inventoryManager.GetCurrencyData().ToString("F2");

      ClearRafineryResources();
      ClearRafineryQueue();
      PopulateRafineryResources();
      PopulateRafineryQueue();
      CheckBusySlots();
      Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });

    }

    public override void CheckOnEnable()
    {
      Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
    }

    public override void CheckInputHandler(Controls controls)
    {
      base.CheckInputHandler(controls);
      if (controls.Player.CallMenu.WasPressedThisFrame())
        _screensManager.OpenScreen<GameHud>().Forget();
    }

    private void Back()
    {
      _screensManager.OpenScreen<GameHud>().Forget();
      //Close();
      //Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
    }

    private void CheckBusySlots()
    {
      var freeSlots = GetFreeSlot();
      foreach (var slot in _itemSlots)
        slot.SetInteractable(freeSlots.countFree > 0);
    }

    private void ClearRafineryQueue()
    {
      foreach (var slot in _rafinerySlots)
        slot.Populate();
    }

    private void ClearRafineryResources()
    {
      for (int i = _resourcesContext.childCount - 1; i >= 0; i--)
        Destroy(_resourcesContext.GetChild(i).gameObject);
      _itemSlots?.Clear();
    }

    private void PopulateRafineryResources()
    {
      foreach (var item in _inventoryManager.GetItemsWithClass("Ore"))
      {
        if (item.count == 0) continue;
        var slot = Instantiate(_itemSlot, _resourcesContext);
        var itemData = _catalogManager.GetResourceData(item.ItemId);
        slot.Populate(item.ItemId, itemData.itemConfig.Icon, (itemData.VirtualCurrencyPrices.SC * 0.01f).ToString("F1"), item.count);
        slot.onSellCallback += SetRafineryData;
        _itemSlots.Add(slot);
      }
    }

    private void PopulateRafineryQueue()
    {
      _rafineryDatas = _inventoryManager.GetRafineryData();
      float total = 0;
      foreach (RafineryData data in _rafineryDatas)
      {
        var slot = GetFreeSlot().slot.Value;
        var itemData = _catalogManager.GetResourceData(data.itemId);
        slot.Populate(data, itemData.itemConfig.Icon, itemData.customData);
        total += data.count * itemData.customData.refiningCost * 0.01f;
      }
      _totalCountText.text = total.ToString("F1");
    }

    private async void SetRafineryData(string itemID, int count, DateTime start)
    {
      if (count == 0) return;
      GetComponent<CanvasGroup>().interactable = false;
      _inventoryManager.SendForRecycling(itemID, count, result =>
      {
        //SuccessAddedRafineryQueue(result);
        _inventoryManager.GetItemData(itemID).count -= count;
        OnEnable();
        GetComponent<CanvasGroup>().interactable = true;
      });
      //if (_waitAnswer == null || !_waitAnswer.gameObject.activeSelf)
      //    _waitAnswer = await _screensManager.OpenPopup<WaitAnswerServerScreen>();
      var itemSlot = InventoryScreenManager.Instance.Inventory.GetItemSlot(itemID);
      if (itemSlot == null) return;
      InventoryScreenManager.Instance.RemoveInventory(itemSlot.GetItemType(), itemSlot.GetItemNum() < count ? itemSlot.GetItemNum() : count);
    }

    private async void GetRafineryCurrency(RafineryData data, Action<bool> onCallback)
    {
      GetComponent<CanvasGroup>().interactable = false;
      if (_waitAnswer == null || !_waitAnswer.gameObject.activeSelf)
        _waitAnswer = await _screensManager.OpenPopup<WaitAnswerServerScreen>();
      int selledCount = 0;
      _inventoryManager.GetFromRecycling(data, async result =>
      {
        var item = _inventoryManager.GetItemData(data.itemId);
        decimal price = item.ItemCustomData.refiningCost * 0.01m/* * data.count*/;
        for (int i = 0; i < data.count; i++)
        {
              if (item.Length <= i)
              {
                  Debug.LogError($"Sell Items. {i}: item.Length = {item.Length}");

                  selledCount++;
                  if (selledCount == data.count)
                  {
                      //InventoryScreenManager.Instance.AddStackableItem(item.ItemId, data.count);
                      //item.count -= data.count;
                      onCallback?.Invoke(true);
                      OnEnable();
                      _screensManager.ClosePopup();
                      _waitAnswer = null;
                      GetComponent<CanvasGroup>().interactable = true;
                      //SuccessAddedRafineryQueue(true, onCallback);
                  }
                  continue;
              }
          _inventoryManager.SellItems(item[i], price, result =>
          {
            selledCount++;
            if (!result) return;
            if (selledCount == data.count)
            {
              //InventoryScreenManager.Instance.AddStackableItem(item.ItemId, data.count);
              //item.count -= data.count;
              onCallback?.Invoke(result);
              OnEnable();
              _screensManager.ClosePopup();
              _waitAnswer = null;
              GetComponent<CanvasGroup>().interactable = true;
              //SuccessAddedRafineryQueue(true, onCallback);
            }
          });
          await Task.Delay(50);
        }
      });
    }

    private void SuccessAddedRafineryQueue(bool result, Action<bool> onCallback = default)
    {
      if (result)
      {
        //_inventoryManager.RefreshInventory(result =>
        //{
        //    //_waitAnswer.Close();
        //    _screensManager.ClosePopup();
        //    _waitAnswer = null;
        //    onCallback?.Invoke(result);
        //    if (result)
        //        OnEnable();
        //    GetComponent<CanvasGroup>().interactable = true;
        //});
      }
      else
      {
        onCallback?.Invoke(false);
        _screensManager.ClosePopup();
        _waitAnswer = null;
        GetComponent<CanvasGroup>().interactable = true;
      }
    }

    private (int countFree, Optionals<QueueRafinerySlot> slot) GetFreeSlot()
    {
      var slots = _rafinerySlots.Where(slot => !slot.IsBusy);
      var count = slots.Count();
      var slot = count > 0 ? new Optionals<QueueRafinerySlot>(slots.First()) : default;
      return (slots.Count(), slot);
    }
  }
}