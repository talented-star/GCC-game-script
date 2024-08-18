using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Zenject;

namespace GrabCoin.Services.Backend
{
    public class BackendServicePostman : IBackEndService
    {
        private const string ContentTypeJson = "application/json";

        private string _apiToken;
        private DiContainer _container;

        public Uri ServerAddress { get; private set; }
        public bool LoggedIn => _apiToken != "";

        public BackendServicePostman(DiContainer container)
        {
            _container = container;
        }

        public void SetServerAddress(string address)
        {
            var newUri = new Uri(address);
            if (newUri != ServerAddress) _apiToken = "";
            ServerAddress = newUri;
        }

        public void SetAuthToken(string token)
        {
            _apiToken = token;
        }

        public async UniTask<IBackEndService.BackendResponseData> ProcessRequest(IBackEndService.BackendRequestData requestData)
        {
            var webRequest = GetRequest(requestData);
            webRequest.SetRequestHeader("Content-Type", ContentTypeJson);
            webRequest.SetRequestHeader("Accept", ContentTypeJson);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestData.Body));

            if (LoggedIn) webRequest.SetRequestHeader("Authorization", $"Bearer {_apiToken}");

            await webRequest.SendWebRequest();

            var result = new IBackEndService.BackendResponseData
            {
                Result = webRequest.result,
                ResponseCode = webRequest.responseCode,
                Body = webRequest.downloadHandler.text
            };

            return result;
        }

        public T CreateRequestHandler<T>() where T : IBackendRequest
        {
            return _container.Instantiate<T>(new object[] {this,});
        }

        private UnityWebRequest GetRequest(IBackEndService.BackendRequestData requestData)
        {
            var fullAddress = new Uri(ServerAddress, requestData.Address);
            switch (requestData.Method)
            {
                case UnityWebRequest.kHttpVerbPOST:
                    return UnityWebRequest.Post(fullAddress, requestData.Body);
                case UnityWebRequest.kHttpVerbGET:
                    return UnityWebRequest.Get(fullAddress);
                case UnityWebRequest.kHttpVerbPUT:
                    return UnityWebRequest.Put(fullAddress, requestData.Body);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}