using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.Helper
{
  public class ColliderHelper : MonoBehaviour
  {
    public Action<Collider> onTriggerEnter = delegate { };
    public Action<Collider> onTriggerExit = delegate { };
    public Action<Collision> onCollisionEnter = delegate { };
    public Action<Collision> onCollisionExit = delegate { };

    private void OnTriggerEnter (Collider other)
    {
      onTriggerEnter?.Invoke(other);
    }

    private void OnTriggerExit (Collider other)
    {
      onTriggerExit?.Invoke(other);
    }

    private void OnCollisionEnter (Collision collision)
    {
      onCollisionEnter?.Invoke(collision);
    }

    private void OnCollisionExit (Collision collision)
    {
      onCollisionExit?.Invoke(collision);
    }
  }
}
