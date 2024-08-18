using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend;
using UnityEngine;
using Zenject;
using GrabCoin.Services.Backend.Inventory;

public class BackendServiceInstaller : MonoInstaller
{
    [SerializeField] private string _serverAddress;

    public override void InstallBindings()
    {
        var service = new BackendServicePostman(Container);
        service.SetServerAddress(_serverAddress);
        Container.Bind<BackendServicePostman>().FromInstance(service).AsSingle();
        Container.Bind<CatalogManager>().FromNew().AsSingle().NonLazy();
        Container.Bind<InventoryDataManager>().FromNew().AsSingle().NonLazy();
    }
}