using Cysharp.Threading.Tasks;
using GrabCoin.UI.Screens;
using System.Net;
using System;
using Assets.Scripts.Code.Common;
using System.IO;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO.Compression;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine;
using Path = System.IO.Path;

namespace GrabCoin.AsyncProcesses
{
    public class LauncherProcess : IAsyncProcess<bool>
    {
        public static Action<object, DownloadProgressChangedEventArgs> OnLoadProgress;
        public static Action<object, System.ComponentModel.AsyncCompletedEventArgs> OnLoadComplete;

        private UniTaskCompletionSource<bool> _completion;
        private PlayerScreensManager _screensManager;
        private LauncherScreen _screen;
        private string _versionGit;
        private string _versionGame;
        private long _zipSize = 0;

        public LauncherProcess(PlayerScreensManager screensManager)
        {
            _screensManager = screensManager;
        }

        public UniTask<bool> Run()
        {
            _completion = new UniTaskCompletionSource<bool>();
            StartProcess();
            return _completion.Task;
        }

        public async void StartProcess()
        {
            await DownloadInfo();

            _screen = await _screensManager.OpenScreen<LauncherScreen>();
            _screen.SetTextInfo(GetInfo());
            
            CheckSelfVersion();

            string nameDir = Directory.GetCurrentDirectory()[0..3];
            long freeSize = GetTotalFreeSpace(nameDir);
            long folderSize = 0;

            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] fiArr = di.GetFiles();
            foreach (FileInfo f in fiArr)
                folderSize += f.Length;
            folderSize += (long)(folderSize * 0.15f);

            APIConnect.GetFileSizeResultsCall(new(GamesData.GetArchiveName()), async result =>
            {
                _zipSize = result.sizeData.size;
                Debug.Log($"Get archive size: {_zipSize} bytes");
                if (freeSize < _zipSize + folderSize)
                {
                    var popup = await _screensManager.OpenPopup<InfoYesNoPopup>();
                    popup.Process($"There's not enough room to update the game. Try to free up some space.\nFree: {freeSize}\nNeed: {_zipSize + folderSize}\nTry again?");
                    popup.onYesClick = () =>
                    {
                        StartProcess();
                    };
                    popup.onNoClick = () =>
                    {
#if UNITY_EDITOR
                        EditorApplication.ExitPlaymode();
#else
                        UnityEngine.Application.Quit();
#endif
                    };
                }
                else
                {
                    var popup = await _screensManager.OpenPopup<InfoYesNoPopup>();
                    popup.Process($"Please download new version to play. {_zipSize} bytes will be downloaded, you need {_zipSize + folderSize} free bytes on disk. Start download now?");
                    popup.onYesClick = () =>
                    {
                        CheckAndCreateGameFolder(_versionGit);

                        OnLoadProgress += WebClient_DownloadProgressChanged;
                        OnLoadComplete += WebClient_DownloadFileCompleted;

                        DownloadAndExtractGame(_versionGit);
                    };
                    popup.onNoClick = () =>
                    {
#if UNITY_EDITOR
                        EditorApplication.ExitPlaymode();
#else
                        UnityEngine.Application.Quit();
#endif
                    };
                }
            }, Debug.LogError);
        }

        public static async Task DownloadReadme()
        {
            using (WebClient client = new WebClient())
                await client.DownloadFileTaskAsync(new Uri(GamesData.GetVersion()), GamesData.GetVersionInGit);
        }

        private async void CheckSelfVersion()
        {
            using (StreamReader streamReader = new StreamReader(@".\" + GamesData.GetVersionInGit))
            {
                string[] data = streamReader.ReadToEnd().Split(' ');
                _versionGit = data[0];
                _versionGit = _versionGit.Replace("\n", "");
            }

            string gameVersionfilePath = @".\" + GamesData.GetVersionInGame;
            if (!File.Exists(gameVersionfilePath))
            {
#if UNITY_EDITOR
                File.Delete(GamesData.GetVersionInGit);
#endif
                var popup = await _screensManager.OpenPopup<InfoPopup>();
                popup.ProcessKey("Unknown version of the game");
                popup.onClose = () =>
                {
                    UnityEngine.Application.OpenURL("https://grabcoinclub.com/");

#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#else
                    UnityEngine.Application.Quit();
#endif
                };
                return;
            }

            using (StreamReader streamReader = new StreamReader(gameVersionfilePath))
            {
                _versionGame = streamReader.ReadToEnd();
            }

            File.Delete(GamesData.GetVersionInGit);
        }

        public static void CheckAndCreateGameFolder(string id)
        {
            string pathFolder = GamesData.GetUnzipFolder;
            if (!Directory.Exists(pathFolder))
            {
                Directory.CreateDirectory(pathFolder);
            }
        }

        public static async Task DownloadInfo()
        {
            using (WebClient client = new WebClient())
                await client.DownloadFileTaskAsync(new Uri(GamesData.GetInfoAddress()), GamesData.GetInfoInGame);
        }

        private string GetInfo()
        {
            using (StreamReader streamReader = new StreamReader(@".\" + GamesData.GetInfoInGame))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static void DownloadAndExtractGame(string id)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) => OnLoadProgress?.Invoke(s, e);

                client.DownloadFileCompleted += (s, e) => OnLoadComplete?.Invoke(s, e);

                client.DownloadFileAsync(new Uri(GamesData.GetUrl()), GamesData.GetZipName);
            }
        }

        private async void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    var popup = await _screensManager.OpenPopup<InfoPopup>();
                    popup.Process($"Download Error\n{e.Error.Message}");
                    popup.onClose = () =>
                    {
                        StartProcess();
                    };
                    return;
                }

                _screen.SetTextProgress("Download Complete");

                var pathZip = Path.Combine(Directory.GetCurrentDirectory(), GamesData.GetZipName);
                long length = new FileInfo(pathZip).Length;
                Debug.Log($"Length downloaded zip archive: {length}");

                if (_zipSize == length)
                {
                    await Task.Delay(2000);
                    _screen.StartExtractProcess();

                    await Task.Run(() => ExtractZip(_versionGit));

                    _screen.StopExtractProcess();
                    await Task.Delay(2000);
                    _screen.SetTextProgress("Extraction Complete");

                    try
                    { Directory.Move(GamesData.GetUnzipFolder, GamesData.GetPath()); }
                    catch { Debug.LogError("Directory.Move(GamesData.GetUnzipFolder, GamesData.GetPath());"); }

                    File.Delete(GamesData.GetZipName);
                    _screen.SetTextProgress("Delete arhive");
                    await Task.Delay(1000);

                    _screen.SetTextProgress("Relaunch game");
                    await Task.Delay(5000);
                    Play();
                }
                else
                {
                    var popup = await _screensManager.OpenPopup<InfoYesNoPopup>();
                    popup.Process($"Error!\nThe archive is corrupted.\n\nTry again?");
                    popup.onYesClick = async () =>
                    {
                        File.Delete(GamesData.GetZipName);
                        _screen.SetTextProgress("Delete arhive");
                        await Task.Delay(1000);
                        StartProcess();
                    };
                    popup.onNoClick = async () =>
                    {
                        File.Delete(GamesData.GetZipName);
                        _screen.SetTextProgress("Delete arhive");
                        await Task.Delay(1000);
#if UNITY_EDITOR
                        EditorApplication.ExitPlaymode();
#else
                        UnityEngine.Application.Quit();
#endif
                    };
                }
            }
            catch (Exception ex)
            {
                var popup = await _screensManager.OpenPopup<InfoPopup>();
                popup.Process(ex.ToString());
            }
        }

        public static void ExtractZip(string id)
        {
            string pathFolder = GamesData.GetUnzipFolder;
            ZipFile.ExtractToDirectory(GamesData.GetZipName, pathFolder);
        }
        private long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (d.IsReady && d.Name == driveName)
                    return d.TotalFreeSpace;
            }
            return -1;
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            _screen.SetDownloadProgress(progress);
        }

        private async void Play()
        {
            try
            {
                var popup = await _screensManager.OpenPopup<InfoPopup>();
                popup.Process("The new version has been downloaded and installed. Game will be closed now. Please run it again to start the updated version.");
                popup.onClose = /* async */ () =>
                {
                    GUIUtility.systemCopyBuffer = $"GCC game is new version";
                    // await Task.Delay(1000);
#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#else
                    UnityEngine.Application.Quit();
#endif
                };
            }
            catch
            {
                try
                {
#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#else
                    UnityEngine.Application.Quit();
#endif
                }
                catch (Exception e)
                {
                    var popup = await _screensManager.OpenPopup<InfoPopup>();
                    popup.Process($"Download Error\n{e}");
                    _completion.TrySetResult(false);
                }
            }
        }
    }
}