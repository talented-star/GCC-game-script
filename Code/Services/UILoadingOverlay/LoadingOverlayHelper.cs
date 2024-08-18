namespace GrabCoin.UI.ScreenManager
{
    public class LoadingOverlayHelper
    {
        private LoadingOverlay _overlay;
        private UIScreensLoader _screensLoader;

        public LoadingOverlayHelper(LoadingOverlay overlay, UIScreensLoader screensLoader)
        {
            _overlay = overlay;
            _screensLoader = screensLoader;

            SubscribeOnScreensLoader();
        }

        private void SubscribeOnScreensLoader()
        {
            _screensLoader.OnLoadingStarted += () => _overlay.Show();
            _screensLoader.OnLoadingCompleted += () => _overlay.Hide();
        }
    }
}