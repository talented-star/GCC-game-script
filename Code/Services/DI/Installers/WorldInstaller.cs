using GrabCoin.GameWorld.Network;
using GrabCoin.GameWorld.Player;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Zenject;

namespace Assets.Scripts.Code.Services.DI.Installers
{
    public class WorldInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var playerNetworkManager = FindObjectOfType<PlayerNetworkManager>();
            Container
                .Bind<PlayerNetworkManager>()
                .FromInstance(playerNetworkManager)
                .AsSingle();

            InstallFactory();
        }

        private void Awake()
        {
            foreach (var pp in FindObjectsOfType<PostProcessVolume>())
                pp.weight = PlayerPrefs.GetFloat("PostprocessingValue", 0.75f);
        }

        private void InstallFactory()
        {
            Container.BindFactory<GameObject, ThirdPersonPlayerController, Factory<ThirdPersonPlayerController>>().FromFactory<ZenFactory<ThirdPersonPlayerController>>();
            Container.BindFactory<GameObject, VRPersonController, Factory<VRPersonController>>().FromFactory<ZenFactory<VRPersonController>>();
            Container.BindFactory<GameObject, NetCloneCharacter, Factory<NetCloneCharacter>>().FromFactory<ZenFactory<NetCloneCharacter>>();
        }
    }
}