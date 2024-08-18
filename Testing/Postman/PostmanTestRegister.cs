using System.Collections;
using System.Collections.Generic;
using GrabCoin.Services.Backend;
using TMPro;
using UnityEngine;
using Zenject;

public class PostmanTestRegister : MonoBehaviour
{
    [Inject] private  BackendServicePostman _service;

    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passField;

    public async void Send()
    {
        var request = _service.CreateRequestHandler<PostmanRequestRegister>();
        request.RequestParameters.email = emailField.text;
        request.RequestParameters.password = passField.text;
        var res = await request.ProcessRequest();
        Debug.Log(res);
    }
}
