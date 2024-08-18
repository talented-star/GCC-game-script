using Code.Services.AuthService;
using Zenject;

namespace GrabCoin.Services.DI.Installers
{
    public class MetamaskAuthInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<MetamaskAuthService>().FromNew().AsSingle();
            Container.Bind<EmailAuthService>().FromNew().AsSingle();
            Container.Bind<CustomIdAuthService>().FromNew().AsSingle();
        }
    }
}