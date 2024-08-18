using UnityEngine;

namespace GrabCoin.GameWorld.Player
{
    public class SyncPositionVithCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void FixedUpdate()
        {
            transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
            float angle = target.rotation.eulerAngles.y;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        }
    }
}
