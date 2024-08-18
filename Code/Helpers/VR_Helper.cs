using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

namespace GrabCoin.Helper
{
  public class VR_Helper : MonoBehaviour
  {
    public static VR_Helper Instance;

    public enum State { Idle, InitializingVR, InitVR_OK, InitVR_Failed };

    private State _state = State.Idle;

    void Awake ()
    {
      if (Instance != null)
      {
        Destroy(this);
        return;
      }
      Instance = this;
    }

    public State GetState ()
    {
      return _state;
    }

    public void EnableXR ()
    {
      StartCoroutine(EnableXR_coroutine());
    }

    private IEnumerator EnableXR_coroutine ()
    {
      _state = State.InitializingVR;
      yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
      if (XRGeneralSettings.Instance.Manager.activeLoader == null)
      {
        _state = State.InitVR_Failed;
        Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
      }
      else
      {
        Debug.Log("Starting XR...");
        XRGeneralSettings.Instance.Manager.StartSubsystems();
        _state = State.InitVR_OK;
        // SpawnVRCharacter();
      }
    }
  }
}