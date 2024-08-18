using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace GrabCoin.Services.Backend
{
    public abstract class PostmanRequestBase<TOut, TIn> : IBackendRequest<TOut, TIn> where TIn : new()
    {
        protected BackendServicePostman _service;
        protected IBackEndService.BackendResponseData _response;
        protected readonly TIn _parameters;
        protected virtual JsonSerializerSettings _serializerSettings => new JsonSerializerSettings();
        protected virtual string Method => UnityWebRequest.kHttpVerbGET;
        protected virtual string Address => "";

        public virtual bool NeedAuthorization => true;
        public TIn RequestParameters => _parameters;
        public UnityWebRequest.Result Result { get; private set; }
        public long ResponseCode { get; private set; }

        public virtual async UniTask<IBackendRequest<TOut, TIn>.Response> ProcessRequest()
        {
            if (_service == null)
            {
                throw new Exception($"{this.GetType()} not properly initialized - no Postman service");
            }

            if (NeedAuthorization && !_service.LoggedIn)
            {
                Result = UnityWebRequest.Result.ConnectionError;
                Debug.LogError($"[PostmanService] No authorization");
                return default;
            }

            Result = UnityWebRequest.Result.InProgress;
            ResponseCode = 0;

            var requestData = new IBackEndService.BackendRequestData
            {
                Method = Method,
                Address = Address,
                Body = ToJson(_parameters)
            };

            _response = await _service.ProcessRequest(requestData);

            Result = _response.Result;
            ResponseCode = _response.ResponseCode;

            if (Result == UnityWebRequest.Result.Success)
            {
                IBackendRequest<TOut, TIn>.Response response = JsonConvert.DeserializeObject<IBackendRequest<TOut, TIn>.Response>(_response.Body);
                return response;
            }
            else
            {
                return default;
            }
        }

        protected virtual string ToJson(TIn parameters)
        {
            return JsonConvert.SerializeObject(parameters, Formatting.None, _serializerSettings);
        }

        protected PostmanRequestBase(BackendServicePostman service)
        {
            _service = service;
            _parameters = new TIn();
        }

    }
}