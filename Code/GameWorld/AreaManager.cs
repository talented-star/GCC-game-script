using UnityEngine;

namespace GrabCoin.GameWorld
{
    public class AreaManager : MonoBehaviour
    {
        public virtual void AreaUpdate() { }
        public virtual void AreaLateUpdate() { }
        public virtual void AreaFixedUpdate() { }
    }
}
