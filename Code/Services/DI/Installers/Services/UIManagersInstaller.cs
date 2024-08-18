using GrabCoin.UI.ScreenManager;
using UnityEngine;
using Zenject;

public class UIManagersInstaller : MonoInstaller
{
    [SerializeField] private UIScreensLoader _loaderPrefab;
    [SerializeField] private UIScreensManager _screensManagerPrefab;
    [SerializeField] private UIGameScreensManager _gameScreensManagerPrefab;
    [SerializeField] private UIPopupsManager _popupsManagerPrefab;
    [SerializeField] private LoadingOverlay _loadingOverlayPrefab;
    [SerializeField] private TransitionsConstructor _transitionsConstructor;

    public override void InstallBindings()
    {
        Container.Bind<UIScreensLoader>().FromComponentInNewPrefab(_loaderPrefab).AsSingle().NonLazy();
        Container.Bind<UIScreensManager>().FromComponentInNewPrefab(_screensManagerPrefab).AsSingle().NonLazy();
        Container.Bind<UIGameScreensManager>().FromComponentInNewPrefab(_gameScreensManagerPrefab).AsSingle().NonLazy();
        Container.Bind<UIPopupsManager>().FromComponentInNewPrefab(_popupsManagerPrefab).AsSingle().NonLazy();
        Container.Bind<TransitionsConstructor>().FromInstance(_transitionsConstructor).AsSingle().NonLazy();
        Container.Bind<LoadingOverlay>().FromComponentInNewPrefab(_loadingOverlayPrefab).AsSingle();
        Container.Bind<LoadingOverlayHelper>().FromNew().AsSingle().NonLazy();
        Container.Bind<PlayerScreensManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
    }
}