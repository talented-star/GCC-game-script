using UnityEngine;

namespace GrabCoin.GameWorld.Resources
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Resource/Resource area stats", fileName = "ResourceAreaStats", order = 51)]
    public class ResourceAreaStats : ScriptableObject
    {
        public string resourceID;
        public GameObject resourcePrefab;
    }
}
