using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectsEnabled : MonoBehaviour
{
    [SerializeField] private GameObject[] _gameObjects;

    private void Update()
    {
        foreach (var gameObject in _gameObjects)
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
    }
}
