using GrabCoin.GameWorld;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.GameWorld
{
    [RequireComponent(typeof(SphereCollider))]
    public class Crater : NetworkBehaviour
    {
        [Serializable]
        private struct Data : NetworkMessage
        {
            public uint id;
            public bool active;
        }

        [SerializeField] private float _cooldown = 6f;
        [SerializeField] private float _power = 40f;
        [SerializeField] private ParticleSystem _burstParticle;
        [SerializeField] private uint _sceneId;
        
        private Data _data;
        private CustomEvent _customEvent;

        private void Awake()
        {
            _data = new Data();
            _data.id = _sceneId;
#if UNITY_SERVER
            _time = UnityEngine.Random.Range(0f, 5f);
#endif
            NetworkClient.RegisterHandler<Data>(OnNetworkMessageReceived);
            _customEvent = OnBurst;
            Translator.Add<GeneralProtocol>(_customEvent);
        }

        private void OnDestroy()
        {
            Translator.Remove<GeneralProtocol>(_customEvent);
            
        }

#if UNITY_SERVER
        private float _time;
        private List<Player.Player> _targetPlayers = new();

        private void Update()
        {
            if (_time < _cooldown)
                _time += Time.deltaTime;
            else
            {
                _time = 0f;
                foreach (Player.Player player in _targetPlayers)
#if UNITY_EDITOR
                    player?.TargetDropOff(_power);
#else
                    player?.DropOff(_power);
#endif
                _targetPlayers.Clear();
                _data.active = true;
                CallRemoteFunction();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponentInParent<Player.Player>();
                if (player != null && !_targetPlayers.Contains(player))
                    _targetPlayers.Add(player);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponentInParent<Player.Player>();
                if (player != null && _targetPlayers.Contains(player))
                    _targetPlayers.Remove(player);
            }
        }

        void CallRemoteFunction()
        {
            NetworkServer.SendToAll(_data);
        }
#endif
        private void OnBurst(System.Enum code, ISendData data)
        {
            if (((IntData)data).value == _sceneId)
                _burstParticle?.Play();
        }

        void OnNetworkMessageReceived(Data netMsg)
        {
            Translator.Send(GeneralProtocol.CraterBurst, new IntData { value = (int)netMsg.id });
        }
    }
}
