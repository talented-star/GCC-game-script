using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.GameWorld
{
    public class FootstepPlayer : MonoBehaviour
    {
        [SerializeField] private List<FootstepTrigger> triggers;
        [SerializeField] private AudioSource source;
        [SerializeField] private List<AudioClip> defaultClips;

        private void Start()
        {
            AddListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            foreach (var item in triggers)
            {
                item.FootstepEvent += OnFootstep;
            }
        }

        private void RemoveListeners()
        {
            foreach (var item in triggers)
            {
                item.FootstepEvent -= OnFootstep;
            }
        }

        private void OnFootstep(Collider collider, Vector3 position)
        {
            if (SurfaceManager.singleton == null || SurfaceManager.singleton.GetFootstep(collider, position) == null)
            {
                source.PlayOneShot(GetRandom(defaultClips));
            }
            else
            {
                source.PlayOneShot(SurfaceManager.singleton.GetFootstep(collider, position));
            }
        }

        private T GetRandom<T>(List<T> list)
        {
            int index = Random.Range(0, list.Count);
            return list[index];
        }
    }
}
