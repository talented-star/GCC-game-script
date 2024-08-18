
namespace GrabCoin.ArenaUI
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.Networking;
    using YoutubeLight;
    using RenderHeads.Media.AVProVideo;
    using Newtonsoft.Json;

    //TODO отрефакторить и причесать

    public class ArenaTelevisions : MonoBehaviour
    {
        public string[] defaultStreamingAssetsVideoList = new string[0];
        public string defaultLivestreamUrl;
        public MediaPlayer mplayer;

        private string currTranslationUrl = string.Empty;
        private IEnumerator liveStreamUrlCoroutine = null;
        private int nextIndexOfLocalVideo = 0;

        private bool streamPlaying = false;

        private void Start()
        {
#if !UNITY_SERVER
            StartCoroutine(GetTranslationUrlFromServer());
#endif
            //TODO Enable livestreamURL
            //GetLivestreamUrl(_livestreamUrl);
        }

        private void ServerTranslationUrlChangedHandler()
        {
            if (string.IsNullOrEmpty(currTranslationUrl))
            {
                streamPlaying = false;
                PlayNextVideoFromStreamingAssets();
            }
            else
            {
                streamPlaying = true;
                mplayer.Stop();
                GetLivestreamUrl(currTranslationUrl);
            }
        }

        private void GetLivestreamUrl(string url)
        {
            if (liveStreamUrlCoroutine != null)
                StopCoroutine(liveStreamUrlCoroutine);

            StartProcess(OnLiveUrlLoaded, url);
        }

        private void StartProcess(System.Action<string> callback, string url)
        {
            StartCoroutine(liveStreamUrlCoroutine = DownloadYoutubeUrl(url, callback));
        }

        private void OnLiveUrlLoaded(string url)
        {
            if (mplayer == null)
                mplayer = GetComponent<MediaPlayer>();

            mplayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, url, true);

            Debug.Log("live url that was passed to the player: " + url);
            streamPlaying = true;
        }

        private void PlayNextVideoFromStreamingAssets()
        {

            if (defaultStreamingAssetsVideoList.Length == 0)
            {
                Debug.LogError("defaultStreamingAssetsVideoList is empty");
                return;
            }
            var path = defaultStreamingAssetsVideoList[nextIndexOfLocalVideo];

            nextIndexOfLocalVideo++;
            if (nextIndexOfLocalVideo >= defaultStreamingAssetsVideoList.Length)
                nextIndexOfLocalVideo = 0;

            mplayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, path, true);
        }

        private IEnumerator DownloadYoutubeUrl(string url, System.Action<string> callback)
        {
            downloadYoutubeUrlResponse = new DownloadUrlResponse();
            var videoId = url.Replace("https://youtube.com/watch?v=", "");

            var newUrl = "https://www.youtube.com/watch?v=" + videoId + "&gl=US&hl=en&has_verified=1&bpctr=9999999999";

            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            downloadYoutubeUrlResponse.httpCode = request.responseCode;
            if (request.isNetworkError) { Debug.Log("Youtube UnityWebRequest isNetworkError!"); }
            else if (request.isHttpError) { Debug.Log("Youtube UnityWebRequest isHttpError!"); }
            else if (request.responseCode == 200)
            {
                if (request.downloadHandler != null && request.downloadHandler.text != null)
                {
                    if (request.downloadHandler.isDone)
                    {
                        downloadYoutubeUrlResponse.isValid = true;
                        downloadYoutubeUrlResponse.data = request.downloadHandler.text;
                    }
                }
                else { Debug.Log("Youtube UnityWebRequest Null response"); }
            }
            else
            {
                Debug.Log("Youtube UnityWebRequest responseCode:" + request.responseCode);
            }

            Debug.Log(request.downloadHandler.text);

            StartCoroutine(liveStreamUrlCoroutine = GetUrlFromJson(callback, videoId, request.downloadHandler.text));
        }

        private IEnumerator GetUrlFromJson(System.Action<string> callback, string _videoID, string pageSource)
        {
            var videoId = _videoID;
            videoId = videoId.Replace("https://www.youtube.com/watch?v=", "");
            videoId = videoId.Replace("https://youtube.com/watch?v=", "");

            var player_response = string.Empty;
            bool tempfix = false;

            if (Regex.IsMatch(pageSource, @"[""\']status[""\']\s*:\s*[""\']LOGIN_REQUIRED") || tempfix)
            {
                var url = "https://www.docs.google.com/get_video_info?video_id=" + videoId + "&eurl=https://youtube.googleapis.com/v/" + videoId + "&html5=1&c=TVHTML5&cver=6.20180913";
                Debug.Log(url);
                UnityWebRequest request = UnityWebRequest.Get(url);
                request.SetRequestHeader("User-Agent", pageSource);
                yield return request.SendWebRequest();
                if (request.isNetworkError) { Debug.Log("Youtube UnityWebRequest isNetworkError!"); }
                else if (request.isHttpError) { Debug.Log("Youtube UnityWebRequest isHttpError!"); }
                else if (request.responseCode == 200)
                {
                    //ok;
                }
                else
                { Debug.Log("Youtube UnityWebRequest responseCode:" + request.responseCode); }
                Debug.Log(request.downloadHandler.text);
                player_response = UnityWebRequest.UnEscapeURL(HTTPHelperYoutube.ParseQueryString(request.downloadHandler.text)["player_response"]);
            }
            else
            {
                var dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;\s*(?:var\s+meta|</script|\n)", RegexOptions.Multiline);
                var dataMatch = dataRegexOption.Match(pageSource);
                if (dataMatch.Success)
                {
                    string extractedJson = dataMatch.Result("$1");
                    if (!extractedJson.Contains("raw_player_response:ytInitialPlayerResponse"))
                    {
                        player_response = JObject.Parse(extractedJson).ToString();
                    }
                }

                dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;\s*(?:var\s+meta|</script|\n)", RegexOptions.Multiline);
                dataMatch = dataRegexOption.Match(pageSource);
                if (dataMatch.Success)
                {
                    player_response = dataMatch.Result("$1");
                }

                dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;\s*(?:var\s+meta|</script|\n)", RegexOptions.Multiline);
                dataMatch = dataRegexOption.Match(pageSource);
                if (dataMatch.Success)
                {
                    player_response = dataMatch.Result("$1");
                }

                dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;", RegexOptions.Multiline);
                dataMatch = dataRegexOption.Match(pageSource);
                if (dataMatch.Success)
                {
                    player_response = dataMatch.Result("$1");
                }
            }

            Debug.Log(player_response);

            JObject json = JObject.Parse(player_response);
            bool isLive = json["videoDetails"]["isLive"].Value<bool>();

            if (isLive)
            {
                string liveUrl = json["streamingData"]["hlsManifestUrl"].ToString();
                Debug.Log(liveUrl);
                callback.Invoke(liveUrl);
            }
            else
            {
                Debug.Log("NO");
                Debug.Log("This is not a livestream url");
            }
        }

        public static void WriteLog(string filename, string c)
        {
            string filePath = Application.persistentDataPath + "/" + filename + "_" + DateTime.Now.ToString("ddMMyyyyhhmmssffff") + ".txt";
            Debug.Log("Log written in: " + Application.persistentDataPath);
            File.WriteAllText(filePath, c);
        }



        private class DownloadUrlResponse
        {
            public string data = null;
            public bool isValid = false;
            public long httpCode = 0;
            public DownloadUrlResponse() { data = null; isValid = false; httpCode = 0; }
        }
        private DownloadUrlResponse downloadYoutubeUrlResponse;




        private IEnumerator GetTranslationUrlFromServer()
        {
            //TODO запрос не пашет, поэтому крутятся только локальные видео
            ServerTranslationUrlChangedHandler();
            yield return null;

            //TODO Включение запроса URL трансляции
            //var updatePeriodSec = 5;
            //var address = "http://185.225.34.121/Client/GetBroadcastUrl";

            //while(true)
            //{
            //    //Debug.Log($"Trying to receave new livestream url from server...");
            //    using (var request = UnityWebRequest.Get(address))
            //    {
            //        yield return request.SendWebRequest();
            //        try
            //        {
            //            if (request.result == UnityWebRequest.Result.Success)
            //            {
            //                var responseText = request.downloadHandler.text;
            //                var serverTranslationUrl = JsonConvert.DeserializeObject<GetUrlResponse>(responseText).data.url;
            //                if (currTranslationUrl != serverTranslationUrl)
            //                {
            //                    var urlForDebug = string.IsNullOrEmpty(serverTranslationUrl) ? "empty string" : serverTranslationUrl;
            //                    Debug.Log($"New liveStreamUrl received from server: {urlForDebug}");
            //                    currTranslationUrl = serverTranslationUrl;
            //                    ServerTranslationUrlChangedHandler();
            //                }
            //                else
            //                {
            //                    //Debug.Log("livestreamurl on server not changed");
            //                }
            //            }
            //            else
            //            {
            //                Debug.Log($"New liveStreamUrl request error: {request.result}");
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            Debug.Log($"Error during recieve URL: {e}");
            //        }
            //    }

            //    yield return new WaitForSeconds(updatePeriodSec);
            //}
        }

        public void VideoPlayerEventHandler()
        {
            if (!streamPlaying)
                PlayNextVideoFromStreamingAssets();
        }

        private class GetUrlResponse
        {
            public Data data;
            public class Data
            {
                public string url;
            }
        }
    }

}