using GrabCoin.Services.Chat;
using GrabCoin.Services.Chat.View;
using GrabCoin.Services.Chat.VoiceChat;
using UnityEngine;
using Zenject;

public class HudInstaller : MonoInstaller
{
    [SerializeField] private ChatWindow chatWindow;
    [SerializeField] private VoiceHudView hudView;


    public override void InstallBindings()
    {
        Container.Bind<ChatWindow>().FromInstance(chatWindow).AsSingle();
        Container.Bind<ChatPresenter>().FromComponentInParents().AsTransient();
        Container.Bind<VoiceHudView>().FromInstance(hudView).AsSingle();
    }
}
