#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace GrabCoin.AsyncProcesses
{
    public abstract class DataClass
    {
        private readonly string Url = "https://admin.grabcoinclub.com/site/";

        protected string url;
        protected byte[] myData;
        protected RequestType requestType;
        protected WWWForm form;

        protected DataClass() { }

        internal abstract IEnumerator CheckFreshToken();

        internal virtual UnityWebRequest GetWebRequest()
        {
            UnityWebRequest webRequest = null;
            switch (requestType)
            {
                case RequestType.GET:
                    webRequest = UnityWebRequest.Get(url);
                    break;
                case RequestType.POST:
                    webRequest = UnityWebRequest.Post(url, form);
                    break;
                case RequestType.PUT:
                case RequestType.PATCH:
                    webRequest = UnityWebRequest.Put(url, myData);
                    break;
            }
            return webRequest;
        }

        public string GetURL()
        {
            return Url;
        }
    }
}