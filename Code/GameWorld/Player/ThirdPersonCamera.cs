using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

namespace GrabCoin.GameWorld.Player
{
    public class ThirdPersonCamera : PivotBasedCameraRig
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        [SerializeField] private float _moveSpeed = 1f;                      // How fast the rig will move to keep up with the target's position.
        [Range(0f, 10f)] [SerializeField] private float _turnSpeed = 1.5f;   // How fast the rig will rotate from user input.
        [SerializeField] private float _turnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        [SerializeField] private float _tiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
        [SerializeField] private float _tiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
        [SerializeField] private bool _verticalAutoReturn = false;           // set wether or not the vertical axis should auto return

        private float _lookAngle;                    // The rig's y axis rotation.
        private float _tiltAngle;                    // The pivot's x axis rotation.
        private const float _LookDistance = 100f;    // How far in front of the pivot the character's look target is.
        private Vector3 _pivotEulers;
        private Quaternion _pivotTargetRot;
        private Quaternion _transformTargetRot;
        private Vector3 _cameraOriginalPos;
        private bool _cameraCorrection;
        private const float _correctionOffset = 0.5f;
        private const float _sphereCastRadius = 0.3f;

        private Controls _controls;
 
        [Inject] private PlayerState playerState;

        [Inject]
        private void Construct(Controls controls, DiContainer diContainer)
        {
            _controls = controls;
        }
 

        protected override void Awake()
        {
            base.Awake();

            _pivotEulers = _pivot.rotation.eulerAngles;

            _pivotTargetRot = _pivot.transform.localRotation;
            _transformTargetRot = transform.localRotation;

            _cameraOriginalPos = _cam.transform.localPosition;
        }
        protected override void  Start()
        {
            base.Start();
 
        }

        protected void Update()
        {
            HandleRotationMovement();
            CheckPhysics();
        }

        private void OnDisable()
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            //Debug.Log($"<color=blue>Cursor visible: {Cursor.visible}</color>");
        }


        protected override void FollowTarget(float deltaTime)
        {
            if (_target == null) return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, _target.position, deltaTime * _moveSpeed);
        }


        private void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            if (playerState.IsMenuActive)
                return;

            // Read the user input
            var moveCamera = _controls.Player.MoveCamera.ReadValue<Vector2>();
            var x = moveCamera.x;
            var y = moveCamera.y;

            // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
            _lookAngle += x * _turnSpeed;

            // Rotate the rig (the root object) around Y axis only:
            _transformTargetRot = Quaternion.Euler(0f, _lookAngle, 0f);

            if (_verticalAutoReturn)
            {
                // For tilt input, we need to behave differently depending on whether we're using mouse or touch input:
                // on mobile, vertical input is directly mapped to tilt value, so it springs back automatically when the look input is released
                // we have to test whether above or below zero because we want to auto-return to zero even if min and max are not symmetrical.
                _tiltAngle = y > 0 ? Mathf.Lerp(0, -_tiltMin, y) : Mathf.Lerp(0, _tiltMax, -y);
            }
            else
            {
                // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
                _tiltAngle -= y * _turnSpeed;
                // and make sure the new value is within the tilt range
                _tiltAngle = Mathf.Clamp(_tiltAngle, -_tiltMin, _tiltMax);
            }

            // Tilt input around X is applied to the pivot (the child of this object)
            _pivotTargetRot = Quaternion.Euler(_tiltAngle, _pivotEulers.y, _pivotEulers.z);

            if (_turnSmoothing > 0)
            {
                _pivot.localRotation = Quaternion.Slerp(_pivot.localRotation, _pivotTargetRot, _turnSmoothing * Time.deltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, _transformTargetRot, _turnSmoothing * Time.deltaTime);
            }
            else
            {
                _pivot.localRotation = _pivotTargetRot;
                transform.localRotation = _transformTargetRot;
            }
        }

        private void CheckPhysics()
        {
            RaycastHit hit;
            var diff = _pivot.TransformPoint(_cameraOriginalPos) - _pivot.position;
            var ray = new Ray(_pivot.position, diff);
            if (Physics.SphereCast(ray, _sphereCastRadius, out hit, diff.magnitude))
            {
                var camPos = ray.GetPoint(hit.distance - _correctionOffset);
                _cam.transform.localPosition = _pivot.InverseTransformPoint(camPos);
                _cameraCorrection = true;
            }
            else if (_cameraCorrection)
            {
                _cam.localPosition = _cameraOriginalPos;
                _cameraCorrection = false;
            }
        }
    }
}


