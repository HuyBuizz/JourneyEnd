using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CommanderAssignUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject assignPanel;
    [SerializeField] private TMP_Dropdown roleDropdown;
    [SerializeField] private TMP_Dropdown playerDropdown;
    [SerializeField] private Button assignButton;
    [SerializeField] private Button closeButton;

    [Header("Other Panels")]
    [SerializeField] private GameObject ObjectivePanel;

    void Start()
    {
        // Sự kiện nút
        assignButton.onClick.AddListener(OnAssignClicked);
        closeButton.onClick.AddListener(Toggle);

        // Setup role dropdown
        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(new List<string> { "Medic", "Engineer", "Firefighter" });

        // Setup player dropdown
        RefreshPlayerList();
    }

    void Update()
    {
        // Chỉ Commander mới mở được panel
        if (PlayerManager.Instance.GetRole() == PlayerRole.Commander)
        {
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                Toggle();
            }
        }
    }

    public void Toggle()
    {
        bool isActive = assignPanel.activeSelf;
        assignPanel.SetActive(!isActive);

        if (!isActive)
        {
            // Khi mở panel → refresh danh sách player
            RefreshPlayerList();

            if (ObjectivePanel != null)
                ObjectivePanel.SetActive(false);

            // Unlock chuột
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (ObjectivePanel != null)
                ObjectivePanel.SetActive(true);

            // Khi đóng panel → lock chuột lại cho gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void RefreshPlayerList()
    {
        playerDropdown.ClearOptions();
        var players = PlayerManager.Instance.GetAllPlayers();
        var options = new List<string>();

        foreach (var p in players)
        {
            options.Add(p.name + (p.isBot ? " [BOT]" : ""));
        }

        playerDropdown.AddOptions(options);
    }

    private void OnAssignClicked()
    {
        // Lấy player được chọn
        int playerIndex = playerDropdown.value;
        ObjMission selectedPlayer = PlayerManager.Instance.GetAllPlayers()[playerIndex];

        // Lấy role từ dropdown
        PlayerRole selectedRole = PlayerRole.Medic; // default
        switch (roleDropdown.value)
        {
            case 0: selectedRole = PlayerRole.Medic; break;
            case 1: selectedRole = PlayerRole.Engineer; break;
            case 2: selectedRole = PlayerRole.Firefighter; break;
        }

        // Gán role cho player
        PlayerManager.Instance.AssignRole(selectedPlayer.id, selectedRole);

        // Gán list mission mặc định cho role
        MissionManager.Instance.AssignDefaultMissionsToPlayer(selectedPlayer.id, selectedRole);

        Debug.Log($"[CommanderUI] Assigned role {selectedRole} to {selectedPlayer.name} with default missions");

        // Refresh MissionUI của player đó (nếu cần)
        var missionUI = Object.FindFirstObjectByType<MissionUIController>();
        if (missionUI != null)
            missionUI.RefreshUI();

        // Đóng panel
        Toggle();
    }


}
