using TMPro;
using UnityEngine;

namespace GrabCoin.UI.ScreenManager
{
    public class LoadingOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject content = default;
        [SerializeField] private TMP_Text messageText = default;

        private string DefaultMessageText => "Loading";

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Hide();
        }

        public void Show(string message = "")
        {
            messageText.text = (string.IsNullOrEmpty(message)) ? DefaultMessageText : message;
            content.SetActive(true);
        }

        public void Hide()
        {
            content.SetActive(false);
        }
    }
}