using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace GrabCoin.GameWorld.Player
{
    public class EventSystemsController : MonoBehaviour
    {
        [SerializeField] private GameObject _Flatscreen_EventSystem;
        [SerializeField] private GameObject _VR_EventSystem;

        public static EventSystemsController Instance;

        void Awake ()
        {
            /*
            if (Instance != null)
            {
            Destroy(this);
            return;
            }
            */
            Instance = this;
        }

        public GameObject GetCurrentEventSystem()
        {
            if (_Flatscreen_EventSystem.activeSelf)
            {
                return _Flatscreen_EventSystem;
            }
            else
            {
                return _VR_EventSystem;
            }
        }

        public void SetEventSystemForFlatscreen ()
        {
            _VR_EventSystem.SetActive(false);
            _Flatscreen_EventSystem.SetActive(true);
        }

        public void SetEventSystemForVR ()
        {
            _Flatscreen_EventSystem.SetActive(false);
            _VR_EventSystem.SetActive(true);
        }
    }
}