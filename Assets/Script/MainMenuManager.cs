using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenu;    // MainMenu trong Hierarchy
    public GameObject spawnMenuPanel; // SpawnMenuPanel trong Hierarchy
    public GameObject settingsPanel; // SettingsPanel trong Hierarchy

    [Header("Buttons in MainMenu")]
    public Button newGameButton;   // 1. NewGame
    public Button optionsButton;   // Options (tùy chọn)

    void Start()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OpenMap);
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenSettings);
    }

    public void OpenMap()
    {
        mainMenu.SetActive(false);
        spawnMenuPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void BackToMainMenu()
    {
        spawnMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}