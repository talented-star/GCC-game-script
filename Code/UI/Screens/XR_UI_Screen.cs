using GrabCoin.UI.ScreenManager;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.Screens
{
  public class XR_UI_Screen : UIScreenBase
  {
    [SerializeField] private Button _closeButton;

    public Action OnCloseWin = delegate { };

    private void Awake()
    {
      if (_closeButton != null)
      {
        _closeButton.onClick.AddListener(CloseWinClicked);
      }
        }

        public override void CheckOnEnable()
        {

        }

        private void CloseWinClicked()
    {
      OnCloseWin?.Invoke();
      gameObject.SetActive(false);
    }
  }
}
