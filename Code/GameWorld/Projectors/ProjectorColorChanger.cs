using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.GameWorld.Projectors
{
    public class ProjectorColorChanger : MonoBehaviour
    {
        [SerializeField] private ParticleSystem light;
        [SerializeField] private ParticleSystemRenderer lightRenderer;
        [SerializeField] private float colorRate;
        [SerializeField] private float colorSwitchSpeed;
        [SerializeField] private List<Color> colors;

        private Color targetColor = Color.white;
        private Color startColor = Color.white;

        private float currentColor = 0f;
        private float lerpColor = 0f;

        private void Update()
        {
            if (currentColor >= colorRate)
            {
                SetColorTarget();
            }

            LerpColor();

            currentColor += Time.deltaTime;
        }

        private void SetColorTarget()
        {
            startColor = lightRenderer.material.color;
            targetColor = colors[Random.Range(0, colors.Count)];
            currentColor = 0f;
            lerpColor = 0f;
        }

        private void LerpColor()
        {
            lightRenderer.material.color = Color.Lerp(startColor, targetColor, lerpColor);
            lerpColor += Time.deltaTime * colorSwitchSpeed;
        }
    }
}
