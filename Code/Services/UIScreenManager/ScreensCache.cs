using System;
using System.Collections.Generic;

namespace GrabCoin.UI.ScreenManager
{
    public class ScreensCache
    {
        private Dictionary<Type, Stack<UIScreenBase>> cache = new Dictionary<Type, Stack<UIScreenBase>>();

        public TScreen TakeOrDefault<TScreen>() where TScreen : UIScreenBase
        {
            var type = typeof(TScreen);
            if (cache.TryGetValue(type, out var instances) == false)
                return default(TScreen);
            return (TScreen)instances.Pop();
        }

        public void Add(Type type, UIScreenBase screen)
        {
            if (cache.TryGetValue(type, out var instances) == false)
                cache.Add(type, instances = new Stack<UIScreenBase>());
            instances.Push(screen);
        }
    }
}