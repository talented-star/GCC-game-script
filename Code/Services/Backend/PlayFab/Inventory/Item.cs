using PlayFab.ClientModels;
using System.Collections.Generic;
using GrabCoin.Services.Backend.Catalog;
using PlayFabCatalog;
using System.Linq;
using PlayFab.EconomyModels;
using PlayFab.SharedModels;
using Newtonsoft.Json;

namespace GrabCoin.Services.Backend.Inventory
{
    public class Item
    {
        private ItemInstance item;
        private InventoryItem itemV2;
        public List<ItemInstance> items = new();
        public int count;
        public List<string> instanceIds = new();

        private CatalogManager _catalogManager;
        private ResourcesCustomData _itemCustomData;

        public ResourcesCustomData ItemCustomData
        {
            get
            {
                if (_itemCustomData == null)
                    RefreshData();
                return _itemCustomData;
            }
        }

        public string ItemClass
        {
            get
            {
                if (item != null)
                    return item.ItemClass;
                else if (itemV2 != null)
                {
                    var itemData = _catalogManager.GetItemData(itemV2.Id);
                    if (itemData == null)
                        return "";
                    var optionals = itemData.catalogItem2;
                    if (!optionals.isInit)
                        return "";
                    var itm = optionals.Value;
                    return itm.ContentType;
                }
                return "";
            }
        }

        public string ItemId =>
            item != null ?
                item.ItemId :
                itemV2 != null ?
                    itemV2.Id :
                    "";

        public string DisplayName =>
            item != null ?
                item.DisplayName :
                itemV2 != null ?
                    _catalogManager.GetItemData(itemV2.Id).DisplayName :
                    "";

        public PlayFabBaseModel RefItem =>
            item != null ?
                item :
                itemV2 != null ?
                    itemV2 :
                    null;

        public Dictionary<string, string> CustomData
        {
            get
            {
                if (item != null)
                    return item.CustomData;
                else if (itemV2 != null)
                {
                    var itemData = _catalogManager.GetItemData(itemV2.Id);
                    if (itemData == null)
                        return new();
                    var optionals = itemData.catalogItem2;
                    if (!optionals.isInit)
                        return new();
                    var itm = optionals.Value;
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(itm.DisplayProperties.ToString());// itm.DisplayProperties as Dictionary<string, string>;
                }
                return new();
            }
        }

        public Item(CatalogManager catalogManager, ItemInstance item, int count)
        {
            this.item = item;
            this.count = count;
            _catalogManager = catalogManager;
            if (item == null) return;
            instanceIds.Add(item.ItemInstanceId);
            items.Add(item);
        }

        public Item(CatalogManager catalogManager, InventoryItem item, int count = 1)
        {
            itemV2 = item;
            this.count = count;
            _catalogManager = catalogManager;
        }

        public Item(CatalogManager catalogManager)
        {
            item = null;
            itemV2 = null;
            count = 0;
            _catalogManager = catalogManager;
        }

        public ItemData GetItemData() =>
            _catalogManager.GetItemData(ItemId);

        public void RefreshData()
        {
            if (item?.ItemClass == "Ore")
                _itemCustomData = _catalogManager.GetResourceData(ItemId).customData;
        }

        public void Add(ItemInstance item)
        {
            if (!instanceIds.Contains(item.ItemInstanceId))
                instanceIds.Add(item.ItemInstanceId);
            if (!items.Contains(item))
                items.Add(item);
        }

        public void Substract(string instanceId)
        {
            if (instanceIds.Contains(instanceId))
                instanceIds.Remove(instanceId);
            var instances = items.Where(item => item.ItemInstanceId == instanceId);
            if (instances.Count() > 0)
                items.Remove(instances.First());
        }

        public int Length =>
            instanceIds.Count;

        public string this[int index] =>
            instanceIds[index];
    }
}