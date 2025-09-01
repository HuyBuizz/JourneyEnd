using UnityEngine;

public class SettingsTabManager : MonoBehaviour
{
    [Header("Các nút điều hướng")]
    public GameObject graphicButton;
    public GameObject audioButton;
    public GameObject controlButton;
    public GameObject performanceButton;
    public GameObject languageButton;

    [Header("Các panel nội dung")]
    public GameObject graphicPanel;
    public GameObject audioPanel;
    public GameObject controlPanel;
    public GameObject performancePanel;
    public GameObject languagePanel;

    [Header("Menu chính & Settings Panel")]
    public GameObject mainMenu;
    public GameObject settingsPanel;

    [Header("Image của các button")]
    public GameObject graphicButtonImage;
    public GameObject audioButtonImage;
    public GameObject controlButtonImage;
    public GameObject performanceButtonImage;
    public GameObject languageButtonImage;

    private GameObject currentPanel;

    void Start()
    {
        ShowPanel(graphicPanel); // mặc định mở Graphic
    }

    public void ShowPanel(GameObject panel)
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);

        panel.SetActive(true);
        currentPanel = panel;

        UpdateButtonImages();
    }

    // bật/tắt image button dựa trên panel đang mở
    private void UpdateButtonImages()
    {
        graphicButtonImage.SetActive(currentPanel == graphicPanel);
        audioButtonImage.SetActive(currentPanel == audioPanel);
        controlButtonImage.SetActive(currentPanel == controlPanel);
        performanceButtonImage.SetActive(currentPanel == performancePanel);
        languageButtonImage.SetActive(currentPanel == languagePanel);
    }

    public void OnGraphicButton() => ShowPanel(graphicPanel);
    public void OnAudioButton() => ShowPanel(audioPanel);
    public void OnControlButton() => ShowPanel(controlPanel);
    public void OnPerformanceButton() => ShowPanel(performancePanel);
    public void OnLanguageButton() => ShowPanel(languagePanel);

    // nút quay lại
    public void OnBackButton()
    {
        settingsPanel.SetActive(false);
        mainMenu.SetActive(true);
    }
}
