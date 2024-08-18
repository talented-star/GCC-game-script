using GrabCoin.Model;
using Zenject;

namespace GrabCoin.Services.DI.Installers
{
    public class UserModelInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<UserModel>()
                .To<UserModelLocal>()
                .FromNew()
                .AsSingle()
                .NonLazy();
        }
    }
}