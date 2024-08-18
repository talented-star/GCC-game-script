using GrabCoin.AIBehaviour;
using GrabCoin.UI.Screens;
using Sources;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "MainScriptableObjectInstaller", menuName = "Installers/MainScriptableObjectInstaller")]
public class MainScriptableObjectInstaller : ScriptableObjectInstaller<MainScriptableObjectInstaller>
{
    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private SettingsConfig settingsConfig;
    [SerializeField] private ItemsConfig itemsConfig;
    [SerializeField] private EnemyBehaviours enemyBehaviours;

    public override void InstallBindings()
    {
        Container.BindInstances(
            audioConfig,
            settingsConfig,
            itemsConfig,
            enemyBehaviours
            );

        CheckInit();
    }

    private void CheckInit()
    {
        settingsConfig.CurrentLanguage = settingsConfig.CurrentLanguage;
    }
}