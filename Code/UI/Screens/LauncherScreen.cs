using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using static Codice.Client.Common.Servers.RecentlyUsedServers;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/LauncherScreen.prefab")]
    public class LauncherScreen : UIScreenBase
    {
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Image _progressImage;
        [SerializeField] public Sprite _infoIcon;

        private bool _isExtracting;

        public override void CheckOnEnable()
        {

        }

        internal void SetTextInfo(string infoText)
        {
            _infoText.text = infoText;
        }

        internal void SetTextProgress(string text)
        {
            _progressText.text = text;
        }

        internal void SetDownloadProgress(int percent)
        {
            _progressText.text = $"Download {percent}%";
            _progressImage.fillAmount = percent * 0.01f;
        }

        internal void StartExtractProcess() =>
            StartCoroutine(ExtractProcess());

        internal void StopExtractProcess()
        {
            _isExtracting = false;
            //StopCoroutine(ExtractProcess());
            SetTextProgress("Extraction Complete");
        }

        internal IEnumerator ExtractProcess()
        {
            _isExtracting = true;
            while (_isExtracting)
            {
                _progressText.text = $"Extracting .";
                yield return new WaitForSeconds(1f);
                if (!_isExtracting) break;
                _progressText.text = $"Extracting ..";
                yield return new WaitForSeconds(1f);
                if (!_isExtracting) break;
                _progressText.text = $"Extracting ...";
                yield return new WaitForSeconds(1f);
            }
        }
    }
}