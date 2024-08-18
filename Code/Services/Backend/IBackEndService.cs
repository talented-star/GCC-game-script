using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace GrabCoin.Services.Backend
{
    public interface IBackEndService
    {
        public Uri ServerAddress { get; }
        public bool LoggedIn { get; }

        public void SetServerAddress(string address);
        public void SetAuthToken(string token);
        public UniTask<BackendResponseData> ProcessRequest(BackendRequestData requestData);

        public T CreateRequestHandler<T>() where T : IBackendRequest;

        public struct BackendRequestData
        {
            public string Address;
            public string Method;
            public string Body;
        }

        public struct BackendResponseData
        {
            public UnityWebRequest.Result Result;
            public long ResponseCode;
            public string Body;
        }
    }
}