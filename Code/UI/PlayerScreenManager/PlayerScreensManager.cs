using Cysharp.Threading.Tasks;
using GrabCoin.UI;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PlayerScreensManager : MonoBehaviour
{
    private static PlayerScreensManager _instance;

    private UIScreenBase _currentScreen;
    private UIScreenBase _currentPopup;
    private UIScreenBase _back;

    private Controls _controls;
    private UIGameScreensManager _gameScreensManager;
    private UIPopupsManager _popupsManager;

    public static PlayerScreensManager Instance
    {
        get 
        {
            if(_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerScreensManager>();
            }
            return _instance;
        }
    }

    [Inject]
    private void Construct(
        Controls controls,
        UIGameScreensManager gameScreensManager,
        UIPopupsManager popupsManager)
    {
        _controls = controls;
        _gameScreensManager = gameScreensManager;
        _popupsManager = popupsManager;
    }

    private void Awake()
    {
        _instance = this;
    }

    public void RegisterScreen<TScreen>(TScreen screen) where TScreen : UIScreenBase
    {
        _gameScreensManager.RegisterScreen(screen);
    }

    public bool EqualsCurrentScreen<TScreen>() where TScreen : UIScreenBase
    {
        if (_currentScreen != null && _currentScreen is TScreen && _currentScreen is GameHud hud)
            return !hud.ChatIsOpened;
        return _currentScreen != null && _currentScreen is TScreen;
    }

    public bool EqualsCurrentPopup<TScreen>() where TScreen : UIScreenBase
    {
        return _currentPopup != null && _currentPopup is TScreen;
    }

    public async UniTask<TScreen> OpenScreen<TScreen>() where TScreen : UIScreenBase
    {
        var screen = await _gameScreensManager.Open<TScreen>();
        //Debug.Log("Open " + screen);
        _currentScreen = screen;
        return screen;
    }

    public async UniTask<TScreen> CreateScreen<TScreen>() where TScreen : UIScreenBase
    {
        var screen = await _gameScreensManager.GetScreenInstance<TScreen>();
        screen.gameObject.SetActive(false);
        //Debug.Log("Create " + screen);
        //_currentScreen = screen;
        return screen;
    }

    public async UniTask<TScreen> OpenPopup<TScreen>() where TScreen : UIScreenBase
    {
        //Debug.Log("OpenPopup");
        var screen = await _popupsManager.Open<TScreen>();
        _currentPopup = screen;
        return screen;
    }

    public void ClosePopup()
    {
        _currentPopup?.Close();
        _currentPopup = null;
        _currentScreen.CheckOnEnable();
    }

    private void Update()
    {
        if (_currentPopup)
            _currentPopup.CheckInputHandler(_controls);
        else if (_currentScreen)
            _currentScreen.CheckInputHandler(_controls);
    }

    public bool CanBeOpened(UIScreenBase screen)
    {
        if (_currentScreen == null || (screen == null && _currentScreen == null) || (_currentScreen != null && screen == _currentScreen))
        {
            return true;
        }
        return false;
    }

    internal async UniTask WaitCurrentTransition()
    {
        await _gameScreensManager.WaitCurrentTransition();
    }
}
