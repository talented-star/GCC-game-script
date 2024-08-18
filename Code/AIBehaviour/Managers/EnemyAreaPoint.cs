using UnityEngine;
using Zenject;

namespace GrabCoin.AIBehaviour
{
    public class EnemyAreaPoint : MonoBehaviour
    {
        [SerializeField] private AreaStats _areaStats;

        public AreaStats AreaStats => _areaStats;

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
                case AreaManagerProtocol.FindEnemyAreaPoints:
                    return new ObjectData { value = this };
            }
            return null;
        }
#endif

#if UNITY_EDITOR
        public bool isShowGizmos;
        private void OnDrawGizmos()
        {
            if (!isShowGizmos) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 1f);
            Gizmos.DrawWireSphere(transform.position, _areaStats._patrulDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _areaStats._pursuitDistance);
        }
#endif
    }
}
