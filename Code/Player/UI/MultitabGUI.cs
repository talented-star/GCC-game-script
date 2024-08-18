using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.Multitab
{
    public class MultitabGUI : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private MultitabAsset[] multitabsAssets;
        [SerializeField] private MultitabPreset[] multitabsPresets;

        [Header("References")]
        [SerializeField] private Transform screensContent;
        [SerializeField] private Transform buttonsContent;
        [SerializeField] private Button buttonPrefab;

        private List<MultitabScreen> screens;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            screens = new();

            for (int i = 0; i < multitabsAssets.Length; i++)
            {
                //Initizalization
                var tab = multitabsAssets[i].multitabScreen;
                screens.Add(tab);
                tab.Initialize();

                //Buttons
                var button = multitabsAssets[i].button;
                int id = screens.Count - 1;
                button.onClick.AddListener(delegate { ActivateScreen(id); });
            }
            
            for (int i = 0; i < multitabsPresets.Length; i++)
            {
                //Initizalization
                var tab = Instantiate(multitabsPresets[i].multitabScreenPrefab, screensContent);
                screens.Add(tab);
                tab.Initialize();

                //Buttons
                var button = Instantiate(buttonPrefab, buttonsContent);
                button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = multitabsPresets[i].name;
                int id = screens.Count - 1;
                button.onClick.AddListener(delegate { ActivateScreen(id); });
            }

            if(screens.Count > 0)
                ActivateScreen(0);
        }

        public void ActivateScreen(int id)
        {
            for (int i = 0; i < screens.Count; i++)
            {
                if (i != id)
                {
                    if (screens[i].gameObject.activeSelf)
                    {
                        screens[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (!screens[i].gameObject.activeSelf)
                    {
                        screens[i].gameObject.SetActive(true);
                    }
                }
            }
        }
    }
    [System.Serializable]
    public class MultitabPreset
    {
        public string name;
        public MultitabScreen multitabScreenPrefab;
    }
    
    [System.Serializable]
    public class MultitabAsset
    {
        public Button button;
        public MultitabScreen multitabScreen;
    }
}