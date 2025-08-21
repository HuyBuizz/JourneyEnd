using UnityEngine;

public class SettingToggle : MonoBehaviour
{
    public GameObject settingsPanel;   // Panel cài đặt
    public GameObject mainMenu;        // Panel menu chính

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);   // bật SettingsPanel
        mainMenu.SetActive(false);       // ẩn MainMenu
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);  // tắt SettingsPanel
        mainMenu.SetActive(true);        // hiện lại MainMenu
    }
}
