using GrabCoin.Services.Chat.View;
using GrabCoin.UI;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenOverlayManager : MonoBehaviour
{
    private List<UIScreenBase> screens = new List<UIScreenBase>();

    private static ScreenOverlayManager _instance;
    public static ScreenOverlayManager Instance
    {
        get
        {
            if(!_instance)
                _instance = FindAnyObjectByType<ScreenOverlayManager>();
            return _instance;
        }
    }

    private static UIScreensManager _instanceScreen;
    public static UIScreensManager InstanceScreen
    {
        get
        {
            if(!_instanceScreen)
                _instanceScreen = FindAnyObjectByType<UIScreensManager>();
            return _instanceScreen;
        }
    }

    public static UIScreenBase GetActiveWindow()
    {
        foreach (Transform child in InstanceScreen.transform)
            if (child.gameObject.activeSelf)
                return child.GetComponent<UIScreenBase>();

        for (int i = 0; i < Instance.screens.Count; i++)
        {
            if (!Instance.screens[i])
            {
                Instance.Fill();
            }
            else if (Instance.screens[i] is ChatWindow || Instance.screens[i] is InventoryScreenManager)
            {
                if(Instance.screens[i] is InventoryScreenManager)
                {
                    var s = Instance.screens[i] as InventoryScreenManager;
                    if (s.Inventory.anim.GetBool("Show"))
                        return Instance.screens[i];
                }
                else
                {
                    var s1 = Instance.screens[i] as ChatWindow;
                    if (s1.ChatIsOpen)
                        return Instance.screens[i];
                }

            }
            else if (Instance.screens[i]!=null&&Instance.screens[i].gameObject.activeInHierarchy)
            {
                return Instance.screens[i];
            }
        }
        return null;
    }

    private void Start()
    {
        Fill();
    }
    private void Fill()
    {
        screens = new List<UIScreenBase>
        {
            LaboratoryScreen.Instance,
            InventoryScreenManager.Instance,
            InGameMenu.Instance,
            ChatWindow.Instance
        };
    }
}
