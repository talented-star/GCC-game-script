using UnityEngine;
using DG.Tweening;
using GrabCoin.GameWorld.Player;

namespace GrabCoin.GameWorld
{
    public class DoorView : MonoBehaviour
    {
        [SerializeField] private Transform doorTransform;
        [SerializeField] private float moveHeight;
        [SerializeField] private float moveSpeed;
        [SerializeField] private AudioSource audioSource;

        private Vector3 closePosition;
        private Vector3 openPosition;

        private int playersCount;

        private Tween moveTween;

        private void Start()
        {
            closePosition = doorTransform.position;
            openPosition = doorTransform.position - Vector3.up * moveHeight;
        }

        private void OnTriggerEnter(Collider other)
        {
            ThirdPersonCharacter character = other.GetComponent<ThirdPersonCharacter>();
            if (character)
            {
                if (playersCount == 0)
                {
                    playersCount++;
                    Move(openPosition);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ThirdPersonCharacter character = other.GetComponent<ThirdPersonCharacter>();
            if (character)
            {
                playersCount--;
                if (playersCount == 0)
                {
                    Move(closePosition);
                }
            }
        }

        private void Move(Vector3 target)
        {
            moveTween.Kill();
            audioSource.Play();
            moveTween = doorTransform.DOMove(target, moveSpeed).SetSpeedBased().OnComplete(() => audioSource.Stop());
        }
    }
}
