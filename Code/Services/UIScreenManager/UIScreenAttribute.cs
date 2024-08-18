using System;

namespace GrabCoin.UI.ScreenManager
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIScreenAttribute : Attribute
    {
        public string AddressableKey { get; private set; }
        public bool LazyLoad { get; private set; }

        public UIScreenAttribute(string addressableKey, bool lazyLoad = false)
        {
            AddressableKey = addressableKey;
            LazyLoad = lazyLoad;
        }
    }
}