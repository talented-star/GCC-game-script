using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class VRPersonController : MonoBehaviour
{
  /*
  private XROrigin _xrOrigin;


  private void Awake()
  {
    _xrOrigin = gameObject.GetComponentInChildren<XROrigin>();

    CharacterMoveHelper moveHelper = _xrOrigin.GetComponent<CharacterMoveHelper>();
    Rigidbody rb = _xrOrigin.GetComponent<Rigidbody>();
    Destroy(moveHelper);
    Destroy(rb);

    string prefabName = "NewVrAvatar"; // "YANA/Prefabs/NewVrAvatar";
    GameObject vrAvatarPrefab = (GameObject)Resources.Load(prefabName, typeof(GameObject));
    if (vrAvatarPrefab == null)
    {
      Debug.Log($"Error loading prefab \"{prefabName}\"");
    }
    else
    {
      Debug.Log($"Prefab \"{prefabName}\" loaded successfully");
      Instantiate(vrAvatarPrefab, Vector3.zero, Quaternion.identity, transform.parent);
      Destroy(gameObject);
    }
  }
  */
}
