using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GrabCoin.Config
{
    public static class ScenePortConfig
    {
        private static Dictionary<string, ushort> _scenePort = new();
        private static string _loadingScene;
        private static string _ip;
        private static bool _isReadComplete;
        private static bool _isReleaseMode;

        private static Dictionary<string, string> _ips = new()
        {
            { "release", "31.129.104.108" },
            { "develop", "185.225.34.121" },
            { "debug", "localhost" }
        };

#if UNITY_SERVER
        public static bool isDayNight;
        public static bool isResources;
        public static Dictionary<string, bool> isEnemyActive = new();
#endif

        public static bool IsReleaseVersion => _isReleaseMode;

        public static string GetLoadingScene()
        {
            if (!_isReadComplete)
            {
                _isReadComplete = true;
                if (ReadConfig())
                    return _loadingScene;
                else
                    SetDefault();
            }
            return _loadingScene;
        }

        public static ushort GetPort(string sceneName)
        {
            if (!_isReadComplete)
            {
                _isReadComplete = true;
                if (ReadConfig())
                    return _scenePort[sceneName];
                else
                    SetDefault();
            }
            return _scenePort[sceneName];
        }

        public static string GetIP()
        {
            //if (!_isReadComplete)
            {
                _isReadComplete = true;
                if (ReadConfig())
                    return _ip;
                else
                    SetDefault();
            }
            return _ip;
        }

        private static bool ReadConfig()
        {
            string writePath = @"Modifications.txt";

            if (File.Exists(GetFileLocation(writePath)))
            {
                using (StreamReader sr = new StreamReader(GetFileLocation(writePath), System.Text.Encoding.Default))
                {
                    _scenePort.Clear();
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //Debug.Log(line);
                        string[] ss = line.Split(':');
                        if (ss.Length == 2)
                        {
                            _scenePort.Add(ss[0], UInt16.Parse(ss[1]));
                        }
                        else if (ss.Length >= 3)
                        {
                            if (ss[0] == "LoadingScene")
                                _loadingScene = ss[2];
                            else if (ss[0] == "IP")
                            {
                                _ip = _ips[ss[2]];
                                _isReleaseMode = ss[2].Equals("release");
                            }
#if UNITY_SERVER
                            else if (ss[0] == "server")
                            {
                                var value = Int32.Parse(ss[2]) == 1;
                                switch (ss[1])
                                {
                                    case "resources":
                                        isResources = value;
                                        break;
                                    case "e_beatle":
                                    case "e_clypeosaurus":
                                    case "e_gorosaurus":
                                        isEnemyActive.Add(ss[1], value);
                                        break;
                                }
                            }
#endif
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private static string GetFileLocation(string relativePath)
        {
            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }

        private static void SetDefault()
        {
            _loadingScene = "StartupServer";
            _scenePort = new Dictionary<string, ushort>
            {
                { "StartupServer", 7730 },
                { "Startup", 7730 },
                { "LocIsland", 7731 },
                { "LocArena", 7732 },
                { "LocSkyHarbor", 7733 },
                { "LocZoneX", 7734 },
                { "LocGlitchville", 7735 },
                { "LocNautilus", 7736 },
                { "LocQuantumForum", 7737 }
            };
        }
    }
}
