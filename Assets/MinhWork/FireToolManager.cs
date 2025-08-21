using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireToolManager : MonoBehaviour
{
    public enum FireTool
    {
        None, CO2, Foam, Powder, Water, Axe
    }

    [Header("Tool Settings")]
    public FireTool currentTool = FireTool.None;

    [Header("UI References")]
    public GameObject fireToolMenu;
    public TextMeshProUGUI titleText;
    public KeyCode toggleKey = KeyCode.G;

    [Header("Tool Buttons")]
    public Button btnCO2;
    public Button btnFoam;
    public Button btnPowder;
    public Button btnWater;
    public Button btnAxe;

    private bool isMenuVisible = false;

    void Start()
    {
        // Debug.Log("FireToolManager Started!");
        SetupButtons();
        HideMenu();
    }

    void Update()
    {
        // Bấm G để bật/tắt menu
        if (Input.GetKeyDown(toggleKey))
        {
            // Debug.Log("G key pressed!");
            ToggleMenu();
        }

        // Bấm phím số để chọn (chỉ khi menu đang hiện)
        if (isMenuVisible)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SelectTool("CO2");
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SelectTool("Foam");
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SelectTool("Powder");
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                SelectTool("Water");
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                SelectTool("Axe");
        }
    }

    void SetupButtons()
    {
        if (btnCO2) btnCO2.onClick.AddListener(() => SelectTool("CO2"));
        if (btnFoam) btnFoam.onClick.AddListener(() => SelectTool("Foam"));
        if (btnPowder) btnPowder.onClick.AddListener(() => SelectTool("Powder"));
        if (btnWater) btnWater.onClick.AddListener(() => SelectTool("Water"));
        if (btnAxe) btnAxe.onClick.AddListener(() => SelectTool("Axe"));
    }

    public void SelectTool(string toolName)
    {
        // Debug.Log("SelectTool called: " + toolName);

        switch (toolName)
        {
            case "CO2":
                currentTool = FireTool.CO2;
                UpdateDisplay("ĐÃ CHỌN: KHÍ CO2");
                break;
            case "Foam":
                currentTool = FireTool.Foam;
                UpdateDisplay("ĐÃ CHỌN: BỌT FOAM");
                break;
            case "Powder":
                currentTool = FireTool.Powder;
                UpdateDisplay("ĐÃ CHỌN: BỘT");
                break;
            case "Water":
                currentTool = FireTool.Water;
                UpdateDisplay("ĐÃ CHỌN: XÔ NƯỚC");
                break;
            case "Axe":
                currentTool = FireTool.Axe;
                UpdateDisplay("ĐÃ CHỌN: RÌU");
                break;
            default:
                currentTool = FireTool.None;
                UpdateDisplay("DỤNG CỤ CHỮA CHÁY");
                break;
        }

        Debug.Log("Selected: " + currentTool);
    }

    void UpdateDisplay(string message)
    {
        if (titleText)
            titleText.text = message;
    }

    public void ToggleMenu()
    {
        // Debug.Log("ToggleMenu called. Current state: " + isMenuVisible);

        if (isMenuVisible)
            HideMenu();
        else
            ShowMenu();
    }

    public void ShowMenu()
    {
        // Debug.Log("ShowMenu called");
        if (fireToolMenu)
        {
            fireToolMenu.SetActive(true);
            isMenuVisible = true;
        }
    }

    public void HideMenu()
    {
        // Debug.Log("HideMenu called");
        if (fireToolMenu)
        {
            fireToolMenu.SetActive(false);
            isMenuVisible = false;
        }
    }
}