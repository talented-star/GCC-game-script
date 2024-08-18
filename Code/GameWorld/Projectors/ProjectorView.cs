using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.GameWorld.Projectors
{
    public class ProjectorView : MonoBehaviour
    {
        [SerializeField] private Transform lightTransform;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float rate;
        [SerializeField] private Vector3 axisMultiplayers;
        [SerializeField] private float clampAngle;

        private Vector3 targetRotation;
        private Vector3 startRotation;
        private float currentRate = 0f;

        private bool isAllowedAngle => Vector3.Dot(lightTransform.forward, startRotation) > 1f - clampAngle / 90f;

        private void Start()
        {
            startRotation = lightTransform.forward;
            SetRotationTarget();
        }

        private void Update()
        {
            if (currentRate >= rate)
            {
                SetRotationTarget();
            }
            lightTransform.Rotate(targetRotation, Time.deltaTime * rotationSpeed);
            CheckAngle();
            currentRate += Time.deltaTime;
        }

        private void SetRotationTarget()
        {
            Vector3 a = Random.onUnitSphere;
            a.z = 0f;
            a.x *= axisMultiplayers.x;
            a.y *= axisMultiplayers.y;
            targetRotation = a;
            currentRate = 0f;
        }

        private void CheckAngle()
        {
            if (!isAllowedAngle)
            {
                InvertTargetRotation();
            }
        }

        private void InvertTargetRotation()
        {
            targetRotation.x = -targetRotation.x;
            targetRotation.y = -targetRotation.y;
            targetRotation.z = -targetRotation.z;
            lightTransform.Rotate(targetRotation, Time.deltaTime * rotationSpeed);
        }

    }
}
