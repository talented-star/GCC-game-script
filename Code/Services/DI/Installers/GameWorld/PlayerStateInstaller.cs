using GrabCoin.GameWorld.Player;
using Zenject;

namespace GrabCoin.Services.DI.Installers
{
    public class PlayerStateInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var playerState = new PlayerState();

            Container.Bind<PlayerState>().FromInstance(playerState).AsSingle().NonLazy();
        }
    }
}