using Sources;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Code.Common
{
    public class SceneNameLogger : MonoBehaviour
    {
        [SerializeField] private string _nameBackgroundMusic = "cityTheme";
        private void Start()
        {
            Debug.Log("--------------SceneStarted-----------------" + SceneManager.GetActiveScene().name);
            if (!string.IsNullOrEmpty(_nameBackgroundMusic))
                AudioManager.Instance.PlayMusic1(_nameBackgroundMusic);
        }
    }
}