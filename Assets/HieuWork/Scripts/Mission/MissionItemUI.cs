using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statusText;
    public Image iconImage;
    public Button selectButton;
    public Slider progressSlider;

    private Mission mission;

    private void Start()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectMission);
    }

    public void SetupMission(Mission mission)
    {
        this.mission = mission;

        if (titleText != null)
            titleText.text = mission.title;

        if (statusText != null)
            statusText.text = GetStatusText(mission.status);

        if (progressSlider != null)
        {
            progressSlider.value = mission.GetProgress();
            progressSlider.gameObject.SetActive(mission.status == MissionStatus.Active);
        }

        Color statusColor = GetStatusColor(mission.status);
        if (statusText != null)
            statusText.color = statusColor;

        if (selectButton != null)
            selectButton.interactable = mission.status == MissionStatus.Active;
    }

    private void OnSelectMission()
    {
        if (mission != null && (mission.status == MissionStatus.Active || mission.status == MissionStatus.Completed))
        {
            MissionManager.Instance.SelectMission(mission);
            FindFirstObjectByType<MissionListUI>()?.HideMissionList();
        }
    }

    private string GetStatusText(MissionStatus status)
    {
        switch (status)
        {
            case MissionStatus.Active: return "ACTIVE";
            case MissionStatus.Completed: return "COMPLETED";
            case MissionStatus.Failed: return "FAILED";
            case MissionStatus.Locked: return "LOCKED";
            default: return "";
        }
    }

    private Color GetStatusColor(MissionStatus status)
    {
        switch (status)
        {
            case MissionStatus.Active: return Color.yellow;
            case MissionStatus.Completed: return Color.green;
            case MissionStatus.Failed: return Color.red;
            case MissionStatus.Locked: return Color.gray;
            default: return Color.white;
        }
    }
}