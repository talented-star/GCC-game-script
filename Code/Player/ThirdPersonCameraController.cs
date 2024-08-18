using DG.Tweening;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GrabCoin.GameWorld.Player
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("Camera Target")] private Transform target;

        [Header("View")]
        [SerializeField, FormerlySerializedAs("View Sensitivity")] private float _viewSensitivity = 3f;
        [SerializeField, FormerlySerializedAs("Scroll Sensitivity")] private float _scrollSensitivity = 3f;
        [SerializeField, FormerlySerializedAs("Maximum Verical Angle")] private float _maxVericalAngle = 70f;
        [SerializeField, FormerlySerializedAs("Minimum Verical Angle")] private float _minVericalAngle = -30f;
        [SerializeField, FormerlySerializedAs("Maximum Distance")] private float _maxDistance = 5f;
        [SerializeField, FormerlySerializedAs("Minimum Distance")] private float _minDistance = 3f;
        [SerializeField, FormerlySerializedAs("Forward Offset")] private float _forwardOffset = 0.1f;
        [SerializeField, FormerlySerializedAs("View Offset")] private Vector2 _viewOffset = new Vector2(0.5f, 0);
        [SerializeField, FormerlySerializedAs("Camera View Smooth Speed")] private float _cameraSmoothSpeed = 15f;
        [SerializeField, FormerlySerializedAs("Camera")] private Camera _camera;
        [SerializeField, FormerlySerializedAs("Default FOV")] private float _defaultFOV = 60f;
        [SerializeField, FormerlySerializedAs("Camera FOV Change Duration")] private float _cameraFovDuration = 0.2f;

        [Space(10)]
        [SerializeField, FormerlySerializedAs("Can Scroll Distance")] private bool _canScrollDistance = false;
        [SerializeField, FormerlySerializedAs("Recoil Smooth")] private float _recoilSmoothTime = 15f;

        [Header("Rays")]
        [SerializeField, FormerlySerializedAs("Rays Distance From Center")] private float _raysDistance = 0.2f;
        [SerializeField, FormerlySerializedAs("Rays")] private Vector2[] _rays;
        [SerializeField, FormerlySerializedAs("Ignore Layer")] private LayerMask _ignoreLayer;

        private float currentDistance = 5f;
        private float targetDistance = 5f;
        private Vector2 currentViewOffset = new Vector2(0.5f, 0f);
        private Vector2 targetViewOffset = new Vector2(0.5f, 0f);
        private bool _isKeyboardMode;
        private CustomEvent _onUIState;
        private float _targetFOV;
        private Tween fovingTween;
        [SerializeField]private Vector2 recoilSmooth;

        public void Reset()
        {
            _rays = new Vector2[9];
            for (int i = 0; i < _rays.Length; i++)
            {
                _rays[i] = new Vector2(Mathf.Cos(i * 45f * Mathf.Deg2Rad), Mathf.Sin(i * 45f * Mathf.Deg2Rad));
            }
            _rays[8] = Vector2.zero;
        }

    // KOSTYL BY YANA: START =========================
        private void Awake()
        {
            _viewSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 3);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _viewSensitivity = 3f;
                PlayerPrefs.SetFloat("mouseSensitivity", _viewSensitivity);
            }
            _targetFOV = _defaultFOV;
        }
    // KOSTYL BY YANA: FINISH =========================

        private void Start()
        {
            transform.parent = null;
            _onUIState = OnChatState;
            Translator.Add<UIPlayerProtocol>(_onUIState);
        }

        private void OnDestroy()
        {
            Translator.Remove<UIPlayerProtocol>(_onUIState);
        }

        public void SetTargetDistance(float distance)
        {
            targetDistance = distance;
        }

        public void SetTargetOffset(Vector2 offset)
        {
            targetViewOffset = offset;
        }

        public void SetDefaultFOV()
        {
            SetFOV(_defaultFOV);
        }
        
        public void SetFOVAmount(float amount)
        {
            SetFOV(_defaultFOV / amount);
        }

        public void SetFOV(float fov)
        {
            if (_targetFOV == fov)
            {
                return;
            }
            if (fovingTween?.IsActive() ?? false)
            {
                fovingTween.Kill();
                fovingTween = null;
            }
            _targetFOV = fov;
            fovingTween = DOTween.To(() => _camera.fieldOfView, x => _camera.fieldOfView = x, _targetFOV, _cameraFovDuration).SetEase(Ease.InCirc);
        }
        private void OnChatState(System.Enum codeEvent, ISendData data)
        {
            switch (codeEvent)
            {
                case UIPlayerProtocol.StateChat:
                case UIPlayerProtocol.OpenGameUI:
                    var state = (BoolData)data;
                    _isKeyboardMode = state.value;
                    break;
            }
        }

        public void ProcessRecoil(Vector2 recoil)
        {
            recoilSmooth += new Vector2(recoil.x, recoil.y);
        }

        private void Update()
        {
            if (!target || _isKeyboardMode)
                return;

            if (_canScrollDistance)
                targetDistance = Mathf.Clamp(targetDistance - Input.GetAxis("Mouse ScrollWheel") * _scrollSensitivity, _minDistance, _maxDistance);

            currentDistance = Mathf.Lerp(currentDistance, targetDistance, _cameraSmoothSpeed * Time.deltaTime);
            currentViewOffset = Vector2.Lerp(currentViewOffset, targetViewOffset, _cameraSmoothSpeed * Time.deltaTime);

            Vector2 delta = recoilSmooth;
            recoilSmooth = Vector2.Lerp(recoilSmooth, Vector2.zero, _recoilSmoothTime * Time.deltaTime);
            delta -= recoilSmooth;
            transform.rotation = Quaternion.Euler(Mathf.Clamp(FixAngle(transform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * _viewSensitivity - delta.y), _minVericalAngle, _maxVericalAngle), transform.eulerAngles.y + Input.GetAxis("Mouse X") * _viewSensitivity + delta.x, 0f);
        }

        private void LateUpdate()
        {
            if (!target)
                return;

            List<RaycastHit> hits = new List<RaycastHit>();
            Vector3 targetDir = GetTargetDirection();

            for (int i = 0; i < _rays.Length; i++)
            {
                var rayPos = GetRayPos(i);
                if (Physics.Raycast(rayPos, targetDir, out var hit, currentDistance, ~_ignoreLayer, QueryTriggerInteraction.Ignore))
                {
                    hits.Add(hit);
                }
            }

            if (hits.Count > 0)
            {
                var closest = hits.Min(a => a.distance);
                UpdateView(closest);
            }
            else
            {
                UpdateView(currentDistance);
            }

        }

        public Vector3 GetRayPos(int i)
        {
            return target.position + transform.rotation * new Vector2(_rays[i].x * _raysDistance, _rays[i].y * _raysDistance);

        }

        public float FixAngle(float angle)
        {
            if (angle > 180)
                return angle - 360f;
            if (angle < -180)
                return angle + 360f;
            return angle;
        }

        public Vector3 GetTargetPosition()
        {
            return transform.rotation * new Vector3(currentViewOffset.x, currentViewOffset.y, -_maxDistance) + target.position;
        }

        public Vector3 GetTargetDirection()
        {
            var targetDir = GetTargetPosition() - target.position;
            targetDir.Normalize();
            return targetDir;
        }

        public void UpdateView(float distance)
        {
            Vector3 targetDir = GetTargetDirection();
            transform.position = targetDir * distance + target.position + transform.forward * _forwardOffset;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!target) return;

            for (int i = 0; i < _rays.Length; i++)
            {
                var rayPos = GetRayPos(i);
                Gizmos.DrawRay(rayPos, transform.rotation * new Vector3(currentViewOffset.x, currentViewOffset.y, -currentDistance));
            }
        }
#endif
    }
}
