using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GrabCoin.Services.Backend
{
    public interface IBackendRequest
    {
        public bool NeedAuthorization { get; }
        public UnityWebRequest.Result Result { get; }
        public long ResponseCode { get; }
    }

    public interface IBackendRequest<TOut, TIn> : IBackendRequest where TIn : new()
    {
        public TIn RequestParameters { get; }
        public UniTask<Response> ProcessRequest();

        public class Response
        {
            public TOut data;
            public string[] errors;
        }
    }
}