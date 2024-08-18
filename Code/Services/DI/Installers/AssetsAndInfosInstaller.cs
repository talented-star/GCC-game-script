using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "MapSpawnPointsInstaller", menuName = "Installers/MapSpawnPointsInstaller")]
public class AssetsAndInfosInstaller : ScriptableObjectInstaller<AssetsAndInfosInstaller>
{
    [SerializeField] private MapSpawnPointsData _mapSpawnPointsData;

    public override void InstallBindings()
    {
        Container.Bind<MapSpawnPointsData>().FromInstance(_mapSpawnPointsData).AsSingle();
    }
}

[CreateAssetMenu(fileName = nameof(MapSpawnPointsData), menuName = "ScriptableObjects/" + nameof(MapSpawnPointsData))]
public class MapSpawnPointsData : ScriptableObject
{
    
}