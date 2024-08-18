using UnityEngine;
namespace GrabCoin.UI.HUD
{
    public class MinimapPoser : MonoBehaviour
    {
        [SerializeField] private Transform center;
        [SerializeField] private Transform top;
        [SerializeField] private MinimapData minimapData;

        private static MinimapPoser instance;
        public static MinimapPoser Instance
        {
            get
            {
                if (instance == null)
                    instance = FindAnyObjectByType<MinimapPoser>();
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        public Vector2 GetPosition(RectTransform map, Vector3 target)
        {
            var dist = Mathf.Abs(center.position.z - top.position.z);

            var targetPos = (center.InverseTransformPoint(target)) / dist;

            return (new Vector2(targetPos.x, targetPos.z) * map.rect.height)/2f;
        }

        public Vector2 GetPivot(Vector3 target)
        {
            var dist = Mathf.Abs(center.position.z - top.position.z);

            var targetPos = (center.InverseTransformPoint(target)) / dist;

            return (Vector2.one / 2f) + new Vector2(targetPos.x, targetPos.z) / 2f;
        }

        public void ApplyPosition()
        {
            if (minimapData)
            {
                top.transform.position = new Vector3(minimapData.top.x, 0, minimapData.top.y);
                center.transform.position = new Vector3(minimapData.center.x, 0, minimapData.center.y);
            }
            else
            {
                Debug.LogError("MINIMAP DATA NOT SET!!!");
            }
        }
        public MinimapData GetMinimapData()
        {
            return minimapData;
        }

        public void SetMinimapData(MinimapData data)
        {
            minimapData = data;
        }
    }
}
