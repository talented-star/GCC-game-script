using GrabCoin.UI.ScreenManager;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GrabCoin.UI.Screens
{
  [UIScreen("UI/Screens/HintVRScreen.prefab")]
  public class HintVRScreen : XR_UI_Screen
  {
    [SerializeField] private TextMeshProUGUI _txtHint;

    public void SetHint (string s)
    {
      _txtHint.text = s;
    }
  }
}