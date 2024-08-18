using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;
using Zenject;

public class NetworkSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] _networkPrefabs;
    [SerializeField] private GameObject _dayNightPrefab;
    [SerializeField] private GameObject _dayNightPrefab2;

    private GameObject _instanceSun;
    private GameObject _instanceMoon;

    private void Awake()
    {
        Spawn();
    }

    [Server]
    private async void Spawn()
    {
        await UniTask.Delay(1000);
        Debug.Log("<<<<<<<<<<<<<Spawn Network Objects>>>>>>>>>>>>>>");
        foreach (var gameObject in _networkPrefabs)
        {

            Debug.Log($"<<<<<<<<<<<<<Create: {gameObject.name}>>>>>>>>>>>>>>");
            var instance = Instantiate(gameObject);
            NetworkServer.Spawn(instance);
        }

        if (_dayNightPrefab != null)
        {
            _instanceSun = Instantiate(_dayNightPrefab, transform);
            NetworkServer.Spawn(_instanceSun);
            Debug.Log($"<<<<<<<<<<<<<Create: {_instanceSun.name}>>>>>>>>>>>>>>");
        }
        if (_dayNightPrefab2 != null)
        {
            _instanceMoon = Instantiate(_dayNightPrefab2, transform);
            NetworkServer.Spawn(_instanceMoon);
            Debug.Log($"<<<<<<<<<<<<<Create: {_instanceMoon.name}>>>>>>>>>>>>>>");
        }
    }
}
