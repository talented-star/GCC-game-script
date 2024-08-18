using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    public class EnemyDriver : MonoBehaviour
    {
        private Vector3 _direction = Vector3.zero;

        public void SetMove(Vector3 direction)
        {
            _direction = direction;
        }

        private void Update()
        {
            transform.Translate(_direction, Space.World);
        }
    }
}
