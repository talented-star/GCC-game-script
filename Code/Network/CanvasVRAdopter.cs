using GrabCoin.UI.Screens;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace GrabCoin.GameWorld.Player
{
  public class CanvasVRAdopter : MonoBehaviour
  {
    [SerializeField] private string[] _canvasNames;
    [SerializeField] private bool[] _canvasIsProcessed;
    [SerializeField] private float[] _scale;
    [SerializeField] private Vector3[] _canvasPosition;
    [SerializeField] private Vector2[] _resizeTo;
    [SerializeField] private string[] _rtNames;
    [SerializeField] private bool[] _rtIsProcessed;
    [SerializeField] private bool[] _rtIsRayRequired;
    [SerializeField] private bool[] _rtIsAvatarStopRequired;
    [SerializeField] private Vector3[] _rtPosition;
    [SerializeField] private bool[] _rtApplyPosition;
    [SerializeField] private Vector3[] _rtRotation;
    [SerializeField] private bool[] _rtApplyRotation;
    [SerializeField] private float _updateDelay = .25f;

    private Transform[] _tCanvas;
    private Transform[] _tOriginalCanvasParent;
    private RectTransform[] _tRT;

    public static CanvasVRAdopter Instance;

    private Camera _camera;
    private Transform _tHead;
    private bool _isCoroutineStarted = false;

    private bool _isRayShown = false;
    private bool _isAvatarMoveBlocked = false;

    void Awake ()
    {
      if (Instance != null)
      {
        Destroy(this);
        return;
      }
      Instance = this;
      DontDestroyOnLoad(gameObject);

      _tCanvas = new Transform[_canvasNames.Length];
      _tOriginalCanvasParent = new Transform[_canvasNames.Length];
      _tRT = new RectTransform[_rtNames.Length];
    }

    /*
    public void Start ()
    {
      StartCoroutine(Update_coroutine(.25f));
    }
    */

    private IEnumerator Update_coroutine (float delay)
    {
      WaitForSeconds waitALittle = new WaitForSeconds(delay);
      while (true)
      {
        if (_tHead != null)
        {
          _AdoptUI2VR();
          CheckIsInteractionRayRequired();
        }
        yield return waitALittle;
      }
    }

    public void RestoreCanvasesParents ()
    {
      for (int i = 0; i < _canvasNames.Length; i++)
      {
        if (!_canvasIsProcessed[i]) { continue; }
        _tCanvas[i].parent = _tOriginalCanvasParent[i];
      }
    }

    public void SetCanvasesParentsForVR (Transform tHead)
    {
      _tHead = tHead;
      for (int i = 0; i < _canvasNames.Length; i++)
      {
        if (!_canvasIsProcessed[i]) { continue; }
        _tCanvas[i].parent = _tHead;
        _tCanvas[i].localPosition = _canvasPosition[i];
        _tCanvas[i].localRotation = Quaternion.identity;
      }
    }

    private void ProcessCanvas (int i)
    {
      GameObject go = GameObject.Find(_canvasNames[i]);
      if (go == null) { return; }
      Canvas canvas = go.GetComponent<Canvas>();
      if (canvas == null) { return; }
      _tCanvas[i] = go.transform;
      _tOriginalCanvasParent[i] = _tCanvas[i].parent;
      SwitchScreenOverlayCanvasToWorldCanvas(canvas, _camera, _tHead, _scale[i], _canvasPosition[i], _resizeTo[i]);
      _canvasIsProcessed[i] = true;
    }

    private void ProcessRectTransform (int i)
    {
      RectTransform rt = _tRT[i];
      if (rt == null)
      {
        GameObject go = GameObject.Find(_rtNames[i]);
        if (go == null)
        {
          // Debug.Log($"===>>>===>>> {_rtNames[i]}: not found");
          return;
        }
        rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
          // Debug.Log($"===>>>===>>> {_rtNames[i]}: RectTransform not found");
          return;
        }
        _tRT[i] = rt;
      }

      if (_rtApplyPosition[i])
      {
        rt.anchoredPosition = _rtPosition[i];
      }
      if (_rtApplyRotation[i]) { rt.localRotation = Quaternion.Euler(_rtRotation[i]); }
      _rtIsProcessed[i] = true;
    }

    public void AdoptUI2VR (Camera camera, Transform tHead)
    {
      _camera = camera;
      SetCanvasesParentsForVR(tHead);
      if (!_isCoroutineStarted)
      {
        StartCoroutine(Update_coroutine(_updateDelay));
        _isCoroutineStarted = true;
      }
      _AdoptUI2VR();
    }

    public void _AdoptUI2VR ()
    {
      if (_canvasNames != null) for (int i = 0; i < _canvasNames.Length; i++)
      {
        if (_canvasIsProcessed[i]) { continue; }
        ProcessCanvas(i);
      }

      if (_rtNames != null) for (int i = 0; i < _rtNames.Length; i++)
      {
        // if (_rtIsProcessed[i]) { continue; }
        ProcessRectTransform(i);
      }
    }

    private void SwitchScreenOverlayCanvasToWorldCanvas (Canvas canvas, Camera camera, Transform tHead, float scale, Vector3 pos, Vector2 resizeTo)
    {
      if (canvas == null) { return; }

      Transform t = canvas.transform;
      canvas.renderMode = RenderMode.WorldSpace;
      canvas.worldCamera = camera;
      t.parent = tHead;
      t.localPosition = pos;
      t.localRotation = Quaternion.identity;
      t.localScale = new Vector3(scale, scale, 1);

      RectTransform rt;
      if ((resizeTo.x > 0) || (resizeTo.y > 0))
      {
        rt = canvas.GetComponent<RectTransform>();
        if (resizeTo.x > 0)
        {
          rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, resizeTo.x);
        }
        if (resizeTo.y > 0)
        {
          rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, resizeTo.y);
        }
      }

      t.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
    }

    private void CheckIsInteractionRayRequired ()
    {
      bool uiIsShown = false;
      bool avatarMustStop = false;
      if (_rtNames != null) for (int i = 0; i < _rtNames.Length; i++)
        {
          if (!_rtIsProcessed[i]) { continue; }
          
          if (_tRT[i].gameObject.activeInHierarchy)
          {
            if (_rtIsRayRequired[i])
            {
              uiIsShown = true;
            }
            if (_rtIsAvatarStopRequired[i])
            {
              avatarMustStop = true;
            }
            // break;
          }
        }
      if (uiIsShown)
      {
        if (!_isRayShown)
        {
          XR_UI.Instance.ShowUIInteractionRay();
          _isRayShown = true;
        }
      }
      else
      {
        if (_isRayShown)
        {
          XR_UI.Instance.HideUIInteractionRay();
          _isRayShown = false;
        }
      }
      if (avatarMustStop)
      {
        if (!_isAvatarMoveBlocked)
        {
          VRPlayerController.Instance.DisableMove();
          _isAvatarMoveBlocked = true;
        }
      }
      else
      {
        if (_isAvatarMoveBlocked)
        {
          VRPlayerController.Instance.EnableMove();
          _isAvatarMoveBlocked = false;
        }
      }
    }
  }
}