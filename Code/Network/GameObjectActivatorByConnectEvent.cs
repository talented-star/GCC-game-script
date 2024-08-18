using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Network
{
    public class GameObjectActivatorByConnectEvent : MonoBehaviour
    {
        [SerializeField] private GameObject _target;
        private PlayerNetworkManager _playerNetwrokManager;


        //private void Awake()
        //{
 
        //    UpdateTarget();
        //}

        //[Inject]
        //private void Construct(PlayerNetworkManager playerNetworkManager)
        //{ 
        //    _playerNetwrokManager = playerNetworkManager;

        //    UpdateTarget();
        //    _playerNetwrokManager.OnConnectedToServer += UpdateTarget;
        //    _playerNetwrokManager.OnDisconnectedFromServer += UpdateTarget;
        //}
 
        //private void OnDestroy()
        //{
        //    if (!_playerNetwrokManager)
        //        return;

        //    _playerNetwrokManager.OnConnectedToServer -= UpdateTarget;
        //    _playerNetwrokManager.OnDisconnectedFromServer -= UpdateTarget;
        //}

        //private void UpdateTarget()
        //{             
        //    bool caclActiveState = _playerNetwrokManager != null &&
        //        _playerNetwrokManager.IsConnected;
             
        //    _target?.SetActive(caclActiveState);
        //}
    }
}