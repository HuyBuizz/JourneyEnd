using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject spawnMenuPanel;
    public GameObject infoPanel;

    [Header("Spawn Points (Buttons)")]
    public Button spawnPoint1Button;
    public Button spawnPoint2Button;
    public Button spawnPoint3Button;

    [Header("Buttons")]
    public Button backButton;
    public Button playButton;

    [Header("Info Display")]
    public TextMeshProUGUI infoText;

    private int selectedSpawnId = -1;

    void Start()
    {
        infoPanel.SetActive(true);
        Image infoImage = infoPanel.GetComponent<Image>();
        if (infoImage != null)
        {
            infoImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Màu đen với opacity 0.5
        }
        UpdateInfoPanel();

        if (spawnPoint1Button != null)
            spawnPoint1Button.onClick.AddListener(() => SelectSpawn(1));
        if (spawnPoint2Button != null)
            spawnPoint2Button.onClick.AddListener(() => SelectSpawn(2));
        if (spawnPoint3Button != null)
            spawnPoint3Button.onClick.AddListener(() => SelectSpawn(3));

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
    }

    private void SelectSpawn(int spawnId)
    {
        selectedSpawnId = spawnId;
        UpdateInfoPanel(); // Cập nhật nội dung khi chọn spawn
        UpdateSpawnHighlights();
    }

    private void UpdateInfoPanel()
    {
        if (selectedSpawnId == -1)
        {
            infoText.text = "Chọn một điểm spawn để bắt đầu!";
            playButton.gameObject.SetActive(false); // Ẩn PlayButton khi chưa chọn
        }
        else
        {
            string info = "";
            switch (selectedSpawnId)
            {
                case 1:
                    info = "Điểm cháy tại nhà dân\nMức độ: Dễ\nSố lượng lửa: 5";
                    break;
                case 2:
                    info = "Điểm cháy tại nhà máy\nMức độ: Trung bình\nSố lượng lửa: 10";
                    break;
                case 3:
                    info = "Điểm cháy tại rừng\nMức độ: Khó\nSố lượng lửa: 15";
                    break;
            }
            infoText.text = info;
            playButton.gameObject.SetActive(true); // Hiện PlayButton khi có chọn
        }
    }

    private void UpdateSpawnHighlights()
    {
        spawnPoint1Button.GetComponent<Image>().color = Color.white;
        spawnPoint2Button.GetComponent<Image>().color = Color.white;
        spawnPoint3Button.GetComponent<Image>().color = Color.white;

        Button selectedButton = GetSpawnButton(selectedSpawnId);
        if (selectedButton != null)
            selectedButton.GetComponent<Image>().color = Color.yellow;
    }

    private Button GetSpawnButton(int id)
    {
        switch (id)
        {
            case 1: return spawnPoint1Button;
            case 2: return spawnPoint2Button;
            case 3: return spawnPoint3Button;
            default: return null;
        }
    }

    public void StartGame()
    {
        if (selectedSpawnId == -1) return;

        PlayerPrefs.SetInt("SpawnId", selectedSpawnId);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        selectedSpawnId = -1;
        UpdateInfoPanel(); // Đảm bảo cập nhật khi quay lại
        UpdateSpawnHighlights();
    }
}