using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MissionUIController : MonoBehaviour
{
    public static MissionUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private TextMeshProUGUI missionDescriptionText;
    [SerializeField] private Slider missionProgressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Lắng nghe sự kiện từ MissionManager
        MissionManager.Instance.OnMissionListChanged += RefreshUI;
        MissionManager.Instance.OnSelectedMissionChanged += RefreshUI;

        // Lắng nghe sự kiện đổi role từ PlayerManager
        PlayerManager.Instance.OnRoleChanged += HandleRoleChanged;

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionListChanged -= RefreshUI;
            MissionManager.Instance.OnSelectedMissionChanged -= RefreshUI;
        }
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnRoleChanged -= HandleRoleChanged;
        }
    }

    private void OnEnable()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionListChanged += RefreshUI;
            MissionManager.Instance.OnSelectedMissionChanged += RefreshUI;
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnRoleChanged += HandleRoleChanged;
        }
    }

    private void OnDisable()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionListChanged -= RefreshUI;
            MissionManager.Instance.OnSelectedMissionChanged -= RefreshUI;
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnRoleChanged -= HandleRoleChanged;
        }
    }

    private void HandleRoleChanged(PlayerRole newRole)
    {
        Debug.Log($"[MissionUI] Role changed → refresh UI for {newRole}");
        RefreshUI();
    }

    public void RefreshUI()
    {
        PlayerRole role = PlayerManager.Instance.GetRole();
        var missions = MissionManager.Instance.GetMissionsForRole(role);

        if (missions == null || missions.Count == 0)
        {
            objectivePanel.SetActive(false);
            return;
        }

        // Lấy mission đang chọn 
        Mission selected = MissionManager.Instance.selectedMission;
        if (selected == null || selected.assignedRole != role)
        {
            selected = missions[0];
        }

        objectivePanel.SetActive(true);
        missionTitleText.text = selected.title;
        missionDescriptionText.text = selected.description;

        // Nếu mission có step hiện tại
        var currentStep = selected.GetCurrentStep();
        if (currentStep != null)
        {
            missionDescriptionText.text = currentStep.description;
        }

        missionProgressBar.value = selected.GetProgress();
    }
}
