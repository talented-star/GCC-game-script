using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using NaughtyAttributes;

namespace GrabCoin.UI.Screens
{
    [CreateAssetMenu(fileName = "ItemsConfig", menuName = "ScriptableObjects/ItemsConfig")]
    public class ItemsConfig : ScriptableObject
    {
        [SerializeField] private List<ItemView> _items;

        public Sprite GetSprite(string key) =>
            _items.FirstOrDefault(item => key == item.Key).Icon;

        public ItemView GetItemView(string key) =>
            _items.FirstOrDefault(item => key == item.Key);

        public ItemView GetItemViewFromName(string name) =>
            _items.FirstOrDefault(item => name == item.name);

        public bool Contains(string key)
            => _items.Any(item => item.Key == key);

        public bool ContainsName(string name)
            => _items.Any(item => item.name == name);

        public void AddOrSet(string key, ItemView itemView)
        {
            for (int i = 0; i < _items.Count; i++)
                if (_items[i].Key == key)
                {
                    _items[i] = itemView;
                    return;
                }
            _items.Add(itemView);
        }
    }

    [Serializable]
    public class ItemView
    {
        public string name;
        [ShowAssetPreview] public Sprite Icon;
        [ShowAssetPreview] public GameObject Prefab;
        public string Key;
    }
}