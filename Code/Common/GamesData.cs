using GrabCoin.Config;
using System.IO;

namespace Assets.Scripts.Code.Common
{
    public class GamesData
    {
        // PRIVATE PROPERTY
        private static string gameFolderPath = 
            //"D:/GCC_Stable/"
#if UNITY_EDITOR
            @".\..\..\Update\";
#else
            @".\..\Update\";
#endif

        private static string gameExecutable =
            @".\..\GrabCoin.bat";

        private static string[] Url = new[]
        {
            $"https://files.grabcoinclub.com/GCC_Release.zip",
            $"https://files.grabcoinclub.com/GCC_Develop.zip"
        };

        private static string[] archiveName =
        {
            "GCC_Release.zip",
            "GCC_Develop.zip"
        };

        private static string[] versionGitAdress =
        {
            "https://files.grabcoinclub.com/READMEVersion.txt",
            "https://files.grabcoinclub.com/DEVELOPVersion.txt"
        };

        private static string infoFileAdress =
            "https://files.grabcoinclub.com/READMEInfo.txt";

        private static string unzipFolder =
#if UNITY_EDITOR
            @".\..\..\ForUpdate\";
#else
            @".\..\ForUpdate\";
#endif
        private static string zipName = @".\test.zip";
        private static string versionFileNameInGIT = "READMEStable.txt";
        private static string versionFileNameInGame = "Version.txt";
        private static string infoFileNameInGame = "Info.txt";

        // PUBLIC PROPERTY

        public static string GetPath() => gameFolderPath;

        public static string GetVersion() => ScenePortConfig.IsReleaseVersion ? versionGitAdress[0] : versionGitAdress[1];

        public static string GetInfoAddress() => infoFileAdress;

        public static string GetExe() => gameExecutable;

        public static string GetUrl() => ScenePortConfig.IsReleaseVersion ? Url[0] : Url[1];
        public static string GetArchiveName() => ScenePortConfig.IsReleaseVersion ? archiveName[0] : archiveName[1];

        public static string GetUnzipFolder => unzipFolder;
        public static string GetZipName => zipName;
        public static string GetVersionInGit => versionFileNameInGIT;
        public static string GetVersionInGame => versionFileNameInGame;
        public static string GetInfoInGame => infoFileNameInGame;
    }
}