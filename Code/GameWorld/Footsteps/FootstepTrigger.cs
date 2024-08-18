using System;
using UnityEngine;

namespace GrabCoin.GameWorld
{
    public class FootstepTrigger : MonoBehaviour
    {
        [SerializeField] private Collider myCollider;

        public event Action<Collider, Vector3> FootstepEvent = delegate { };

        private void Start()
        {
            SetupCollisions();
        }

        private void OnTriggerEnter(Collider other)
        {
            FootstepEvent.Invoke(other, transform.position);
        }

        private void SetupCollisions()
        {
            var colliders = GetComponentsInParent<Collider>();
            foreach (var item in colliders)
            {
                if (item != myCollider)
                {
                    Physics.IgnoreCollision(myCollider, item);
                }
            }
        }
    }
}
