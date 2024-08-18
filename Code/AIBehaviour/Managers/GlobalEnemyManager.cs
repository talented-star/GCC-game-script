using GrabCoin.GameWorld;
using System.Linq;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    public class GlobalEnemyManager : GlobalManager
    {
        private void Start()
        {
            var enemyAreas = Translator.SendAnswers<AreaManagerProtocol, IntData, ObjectData>(AreaManagerProtocol.FindEnemyAreaPoints, new IntData())
                .Select(obj => obj.value as EnemyAreaPoint)
                .ToList();

            foreach (EnemyAreaPoint point in enemyAreas)
            {
                var newObject = _container.InstantiateComponent(typeof(EnemyAreaManager), point.gameObject);
                var area = newObject as EnemyAreaManager;
                area.Init(point.AreaStats);

                _areas.Add(area);
            }

            Debug.Log($"Init Enemy Manager. Count area: {_areas.Count}");
        }
    }
}
