using System.Collections.Generic;

namespace GrabCoin.Services
{
    public static class SceneNameProvider
    {
        private static Dictionary<SceneType, string> _scenes = new Dictionary<SceneType, string>()
        {
            { SceneType.Startup, "Startup"},
            { SceneType.MirrorTestStart, "MirrorTestStartScene"},
            //{ SceneType.Island, "TestScene"},
            { SceneType.Island, "TestScene_Optimization"},
            { SceneType.Arena, "Arena"},
        };

        public static string GetSceneName(SceneType scene) => _scenes[scene];
    }
}