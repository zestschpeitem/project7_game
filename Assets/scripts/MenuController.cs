using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public enum Screen
    {
        None,
        Main,
        Settings
    }

    public CanvasGroup mainScreen;
    public CanvasGroup settingsScreen;

    private void SetCurrentScreen(Screen screen)
    {
        Utility.SetCanvasGroupEnabled(mainScreen, screen == Screen.Main);
        Utility.SetCanvasGroupEnabled(settingsScreen, screen == Screen.Settings);
    }

    void Start()
    {
        SetCurrentScreen(Screen.Main);
    }

    public void StartNewGame()
    {
        SetCurrentScreen(Screen.None);
        LoadingScreen.instance.LoadScene("Game");
    }

    public void StartLevel2()
    {
        SetCurrentScreen(Screen.None);
        LoadingScreen.instance.LoadScene("Level2");
    }

    public void OpenSettings()
    {
        SetCurrentScreen(Screen.Settings);
    }

    public void CloseSettings()
    {
        SetCurrentScreen(Screen.Main);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}


