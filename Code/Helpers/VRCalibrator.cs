using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.Helper
{
  public class VRCalibrator : MonoBehaviour
  {
    public static VRCalibrator Instance;

    private float _avatarHeight;
    private float _avatarLeftHandLength;
    private float _avatarLeftShoulderHeight;
    private float _avatarRightHandLength;
    private float _avatarRightShoulderHeight;

    private float _unscaledAvatarHeight;

    public float AvatarHeight { get { return _avatarHeight; } }
    public float AvatarLeftHandLength { get { return _avatarLeftHandLength; } }
    public float AvatarRightHandLength { get { return _avatarRightHandLength; } }
    public float AvatarLeftShoulderHeight { get { return _avatarLeftShoulderHeight; } }
    public float AvatarRightShoulderHeight { get { return _avatarRightShoulderHeight; } }

    void Awake()
    {
      if (Instance != null)
      {
        Destroy(this);
        return;
      }
      Instance = this;
      DontDestroyOnLoad(this);
    }

    public void StoreCalibrationData(Transform tHead, Transform tLeftHand, Transform tRightHand, float avatarHeight)
    {
      Vector3 posH = tHead.position;
      Vector3 posLH = tLeftHand.position;
      Vector3 posRH = tRightHand.position;

      _avatarLeftShoulderHeight = posLH.y - posH.y + avatarHeight;
      float dx = posH.x - posLH.x;
      float dz = posH.z - posLH.z;
      _avatarLeftHandLength = Mathf.Sqrt(dx * dx + dz * dz);

      _avatarRightShoulderHeight = posRH.y - posH.y + avatarHeight;
      dx = posH.x - posRH.x;
      dz = posH.z - posRH.z;
      _avatarRightHandLength = Mathf.Sqrt(dx * dx + dz * dz);
    }

    public void StoreCalibrationData(float avatarHeight, float avatarLeftHandLength, float avatarRightHandLength, float avatarLeftShoulderHeight, float avatarRightShoulderHeight)
    {
      _avatarHeight = avatarHeight;
      _avatarLeftHandLength = avatarLeftHandLength;
      _avatarLeftShoulderHeight = avatarLeftShoulderHeight;
      _avatarRightHandLength = avatarRightHandLength;
      _avatarRightShoulderHeight = avatarRightShoulderHeight;
    }

    public void CalibrateAvatar(Transform tBody)
    {
      float newScale = _avatarHeight / _unscaledAvatarHeight;
      tBody.localScale = new Vector3(newScale, newScale, newScale);
    }

    public void CalibrateAvatar(Transform tBody, float unscaledAvatarHeight)
    {
      _unscaledAvatarHeight = unscaledAvatarHeight;
      CalibrateAvatar(tBody);
    }

    public void CalibrateAvatarCountingOriginalScale(Transform tBody, float originalScale)
    {
      float newScale = originalScale * _avatarHeight / _unscaledAvatarHeight;
      tBody.localScale = new Vector3(newScale, newScale, newScale);
    }
  }
}