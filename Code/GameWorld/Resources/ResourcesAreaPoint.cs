using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Resources
{
    public class ResourcesAreaPoint : MonoBehaviour
    {
        [SerializeField] private ResourceAreaStats _areaStats;
        [SerializeField] private Transform[] _spawnPoints;
        public ResourceAreaStats AreaStats => _areaStats;
        public Transform[] SpawnPoints => _spawnPoints;
#if UNITY_SERVER

        private CustomAnswerEvent _answerEvent;

        [Inject]
        private void Construct()
        {
            if (!gameObject.activeSelf) return;
            _answerEvent = OnSelfReturnEvent;
            Translator.Add<AreaManagerProtocol>(_answerEvent);
        }

        private void OnDestroy()
        {
            Translator.Remove<AreaManagerProtocol>(_answerEvent);
        }

        private ISendData OnSelfReturnEvent(System.Enum codeEvent, ISendData data)
        {
            switch (codeEvent)
            {
                case AreaManagerProtocol.FindResourcesAreaPoints:
                    return new ObjectData { value = this };
            }
            return null;
        }
#endif

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(transform.position, 1f);
            foreach (var spawnPoint in _spawnPoints)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(spawnPoint.position, 2f);
            }
        }
    }
}
