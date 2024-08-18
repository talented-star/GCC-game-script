using Sources;
using Zenject;

namespace GrabCoin.Services.DI.Installers
{
    public class AudioManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<AudioManager>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("AudioManager")
                .UnderTransform(transform)
                .AsSingle()
                .NonLazy();
        }
    }
}