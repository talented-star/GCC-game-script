using UnityEngine;
using TMPro;
using Mirror;
using System;

namespace GrabCoin.UI.HUD
{
  public class FrameRateCounter : MonoBehaviour
  {
    [SerializeField] private float _updateRate = 1f;
    [SerializeField] private TextMeshProUGUI _txtFPS;
    [SerializeField] private TextMeshProUGUI _txtPing;

    private float _FPS;
    private int _framesNum = 0;
    private float _timer = -1;

    private void UpdateFrameRate (float dt)
    {
      ++_framesNum;
      _timer -= dt;
      if (_timer < 0)
      { 
        _FPS = _framesNum / (_updateRate - _timer);
        _txtFPS.text = $"FPS: {Mathf.Round(_FPS)}";
        _timer = _updateRate;
        _framesNum = 0;
      }
    }

    private void Update()
    {
      UpdateFrameRate(Time.deltaTime);
            _txtPing.text = $"Ping: {Math.Round(NetworkTime.rtt * 1000)}ms";
    }
  }
}