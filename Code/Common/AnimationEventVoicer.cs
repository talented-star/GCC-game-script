using Sources;
using UnityEngine;
using Zenject;

public class AnimationEventVoicer : MonoBehaviour
{
    [SerializeField] private string _footStepKey;

    private AudioManager _audioManager;

    [Inject]
    private void Construct(AudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public void FootStepSound()
    {
        // _audioManager.PlaySound3D(_footStepKey, transform.position);
        AudioManager.Instance.PlaySound3D(_footStepKey, transform.position);
    }

    public void DieSound(string dieSoundKey)
    {
        AudioManager.Instance.PlaySound3D(dieSoundKey, transform.position);
    }
}
