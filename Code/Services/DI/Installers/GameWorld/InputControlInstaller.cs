using Zenject;

namespace GrabCoin.Services.DI.Installers
{
    public class InputControlInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var controls = new Controls();
            controls.Enable();

            Container.Bind<Controls>().FromInstance(controls).AsSingle();

            var keySettings = new KeyInputSettings(controls);
            Container.Bind<KeyInputSettings>().FromInstance(keySettings).AsSingle();
        }
    }
}