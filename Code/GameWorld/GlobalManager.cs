using GrabCoin.GameWorld.Player;
using GrabCoin.UI.Screens;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld
{
    public class GlobalManager : MonoBehaviour
    {
        protected List<Player.Player> _onlinePlayers;
        protected List<Collider> _regions;
        protected List<AreaManager> _areas;
        protected DiContainer _container;
        protected ItemsConfig _itemsConfig;

        [Inject]
        private void Construct(DiContainer container, ItemsConfig itemsConfig)
        {
            _container = container;
            _itemsConfig = itemsConfig;
        }

        private void Awake()
        {
            _onlinePlayers = new();
            _regions = new();
            _areas = new();
        }

        private void Update()
        {
            foreach (AreaManager area in _areas)
                area.AreaUpdate();
        }

        private void LateUpdate()
        {
            foreach (AreaManager area in _areas)
                area.AreaLateUpdate();
        }

        private void FixedUpdate()
        {
            foreach (AreaManager area in _areas)
                area.AreaFixedUpdate();
        }

        public void AddPlayer(Player.Player player)
        {
            if (!_onlinePlayers.Contains(player))
                _onlinePlayers.Add(player);
        }

        public void RemovePlayer(Player.Player player)
        {
            if (_onlinePlayers.Contains(player))
                _onlinePlayers.Remove(player);
        }
    }
}
