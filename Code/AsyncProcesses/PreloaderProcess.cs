using Cysharp.Threading.Tasks;
using GrabCoin.UI.Screens;
using System.Net;
using System;
using Assets.Scripts.Code.Common;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO.Compression;
using System.Linq;

namespace GrabCoin.AsyncProcesses
{
    public class PreloaderProcess : IAsyncProcess<bool>
    {
        private UniTaskCompletionSource<bool> _completion;
        private PlayerScreensManager _screensManager;
        private string versionGit;
        private string _versionGame;
        private bool isVersionValidate = false;

        public PreloaderProcess(PlayerScreensManager screensManager)
        {
            _screensManager = screensManager;
        }

        public UniTask<bool> Run()
        {
            _completion = new UniTaskCompletionSource<bool>();
            Process();
            return _completion.Task;
        }

        private async void Process()
        {
            var screen = await _screensManager.OpenScreen<PreloaderScreen>();
            screen.Process().Forget();

            await DownloadReadme();
            CheckVersion();

//            if (isVersionValidate)
//                foreach (string dir in Directory.GetDirectories(@".\" +
//#if UNITY_EDITOR
//                    "../../"
//#else
//                    "../"
//#endif
//                    ))
//                    if (File.Exists(dir + "/" + GamesData.GetExe(0)))
//                        if (File.Exists(dir + "/" + GamesData.GetVersionInGame))
//                        {
//                            string version = "";
//                            using (StreamReader streamReader = new StreamReader(dir + "/" + GamesData.GetVersionInGame))
//                            {
//                                version = streamReader.ReadToEnd();
//                            }

//                            if (!version.Equals(versionGit))
//                            {
//                                await Task.Delay(3000);
//                                try
//                                {
//                                    Directory.Delete(dir, true);
//                                }
//                                catch { }
//                            }
//                        }

//            try
//            { Directory.Move(Directory.GetCurrentDirectory(), GamesData.GetPath()); }
//            catch { }

            screen.Release();
            _completion?.TrySetResult(isVersionValidate);
        }

        public static async Task DownloadReadme()
        {
            using (WebClient client = new WebClient())
                await client.DownloadFileTaskAsync(new Uri(GamesData.GetVersion()), GamesData.GetVersionInGit);
        }

        private void CheckVersion()
        {
            using (StreamReader streamReader = new StreamReader(@".\" + GamesData.GetVersionInGit))
            {
                string[] data = streamReader.ReadToEnd().Split(' ');
                versionGit = data[0];
                versionGit = versionGit.Replace("\n", "");
            }

            if (GUIUtility.systemCopyBuffer == $"GCC game is new version")
            {
                isVersionValidate = true;
#if UNITY_EDITOR
                File.Delete(GamesData.GetVersionInGit);
#endif
                return;
            }

            string gameVersionfilePath = @".\" + GamesData.GetVersionInGame;
            if (!File.Exists(gameVersionfilePath))
            {
                isVersionValidate = false;
                return;
            }


            using (StreamReader streamReader = new StreamReader(gameVersionfilePath))
            {
                _versionGame = streamReader.ReadToEnd();
            }

            if (versionGit.Equals(_versionGame))
            {
                isVersionValidate = true;
#if UNITY_EDITOR
                File.Delete(GamesData.GetVersionInGit);
#endif
            }
            else
                isVersionValidate = false;
        }
    }
}