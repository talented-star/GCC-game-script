using UnityEngine;

namespace GrabCoin.GameWorld.Network
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private int _id;
        public int Id => _id;
    }
}