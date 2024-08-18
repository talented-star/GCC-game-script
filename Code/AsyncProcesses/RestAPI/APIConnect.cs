using System;
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace GrabCoin.AsyncProcesses
{
    public class APIConnect : MonoBehaviour
    {
        private static APIConnect instance;

        public static void SetTokenBalanceCall(SetTokenBalanceData data, Action<SetTokenBalanceResult> success, Action<string> fail)
            => Call(data, success, fail);

        public static void GetFileSizeResultsCall(GetFileSizeRequestsData data, Action<GetUserRequestsResult> success, Action<string> fail)
            => Call(data, success, fail);

        private static void Call<T>(DataClass data, Action<T> success, Action<string> fail) where T : class, new()
        {
            CreateInstance();
            instance.StartCoroutine(APIRequest(data, success, fail));
        }

        private static IEnumerator APIRequest<T>(DataClass data, Action<T> success, Action<string> fail) where T : class, new()
        {
            UnityWebRequest webRequest = data.GetWebRequest();

            using (webRequest)
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result is
                    UnityWebRequest.Result.ProtocolError or
                    UnityWebRequest.Result.ConnectionError)
                    fail(webRequest.error + "^" + webRequest.downloadHandler.text);
                else
                {
                    Result result = new T() as Result;
                    result.Init(webRequest.downloadHandler.text);
                    success(result as T);
                }
            }
        }

        private static void CreateInstance()
        {
            if (instance == null)
            {
                //find existing instance
                instance = GameObject.FindObjectOfType<APIConnect>();
                if (instance == null)
                {
                    //create new instance
                    var go = new GameObject(nameof(APIConnect));
                    instance = go.AddComponent<APIConnect>();
                    GameObject.DontDestroyOnLoad(go);
                }
            }
        }
    }
}