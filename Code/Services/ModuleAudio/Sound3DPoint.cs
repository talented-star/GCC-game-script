using UnityEngine;

namespace Sources
{
    public class Sound3DPoint
    {
        public GameObject point;
        public AudioSource source;
        public bool isOnlyScene;
        public bool isLoop;

        public Sound3DPoint(Transform parent)
        {
            point = new GameObject("sound3D point");
            point.transform.SetParent(parent);

            source = point.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
        }

        public void PlaySound3D(AudioData clip, Vector3 worldPosition, bool isOnlyScene, bool isLoop)
        {
            point.transform.position = worldPosition;

            source.loop = isLoop;
            source.volume = clip.Volume;
            source.clip = clip.Clip;

            this.isOnlyScene = isOnlyScene;
            source.Play();
        }
    }
}

