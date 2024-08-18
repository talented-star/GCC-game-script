using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GrabCoin.UI.Screens
{
  public class XR_UI : MonoBehaviour
  {
    [SerializeField] private XR_UI_Screen _scrHelp;
    [SerializeField] private XR_UI_Screen _scrNotImplementedYet;
    [SerializeField] private HintVRScreen _scrHint;
    [SerializeField] private XR_UI_Screen _scrLocationLockedForVR;

    [SerializeField] private Transform _tWorldUIHolder;
    [SerializeField] private Transform _tBodyUIHolder;
    [SerializeField] private Transform _tHeadUIHolder;
    [SerializeField] private Transform _tHudUIHolder;

    [SerializeField] private Transform _tHead;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private XRInteractorLineVisual _xrInteractorLineVisual;

    public static XR_UI Instance;

    public enum Screen { Help, NotImplementedYet, Hint, LocationLockedForVr }

    private XR_UI_Screen[] _screens;

    void Awake ()
    {
      if (Instance != null)
      {
        Destroy(this);
        return;
      }
      Instance = this;
      _screens = new XR_UI_Screen[] { _scrHelp, _scrNotImplementedYet, _scrHint, _scrLocationLockedForVR };
      HideAll();
    }

    void Update ()
    {
      _tHudUIHolder.position = _tHead.position;
      _tHudUIHolder.rotation = _tHead.rotation;

      _tHeadUIHolder.position = _tHead.position;
      _tHeadUIHolder.rotation = Quaternion.Euler(0, _tHead.eulerAngles.y, 0);

      _tBodyUIHolder.position = _tHead.position;
    }

    private void OnEnable ()
    {
      _scrHelp.OnCloseWin += OnInfoWindowClose;
      _scrNotImplementedYet.OnCloseWin += OnInfoWindowClose;
      _scrLocationLockedForVR.OnCloseWin += OnInfoWindowClose;
    }

    public void HideAll ()
    {
      HideUIInteractionRay();
      for (int i = 0; i < _screens.Length; i++)
      {
        _screens[i].gameObject.SetActive(false);
      }
    }

    public void ShowScreen (Screen scr)
    {
      HideAll();
      _screens[(int)scr].gameObject.SetActive(true);
      ShowUIInteractionRay();
    }

    public void ShowHint (string s)
    {
      HideAll();
      _screens[(int)Screen.Hint].gameObject.SetActive(true);
      _scrHint.SetHint(s);
    }

    public void HideHint ()
    {
      _screens[(int)Screen.Hint].gameObject.SetActive(false);
    }

    public void OnInfoWindowClose ()
    {
      HideAll();
    }

    public void ShowUIInteractionRay ()
    {
      _lineRenderer.enabled = true;
      _xrInteractorLineVisual.enabled = true;
    }

    public void HideUIInteractionRay ()
    {
      _lineRenderer.enabled = false;
      _xrInteractorLineVisual.enabled = false;
    }
  }
}