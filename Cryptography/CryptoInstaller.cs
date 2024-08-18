using GrabCoin.AsyncProcesses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryptoInstaller : MonoBehaviour
{
    [SerializeField] private string secretKey = "";
    private SHA1 _sha;

    private void Awake()
    {
        SetTokenBalanceData.secretKey = secretKey;
        _sha = new SHA1();
    }
}
