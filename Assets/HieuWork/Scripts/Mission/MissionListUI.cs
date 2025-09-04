using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class MissionListUI : MonoBehaviour
{
    [Header("Mission List Panel - Large UI")]
    public GameObject missionListPanel;
    public Transform mainJobsParent;
    public Transform sideJobsParent;
    public Transform completedParent;
    public Button closeButton;

    [Header("Formatting")]
    [Tooltip("Prefix icon for a completed step item")] public string completedStepIcon = "[OK] ";
    [Tooltip("Prefix icon for an active (current) step item")] public string currentStepIcon = "-> ";
    [Tooltip("Indent before step lines")] public string stepIndent = "    ";

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(HideMissionList);

        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionListChanged += RefreshMissionList;

        if (missionListPanel != null)
            missionListPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionListChanged -= RefreshMissionList;
    }

    public void ShowMissionList()
    {
        if (missionListPanel != null)
        {
            missionListPanel.SetActive(true);
            RefreshMissionList();
        }
        Time.timeScale = 0f; // Pause game
    }

    public void HideMissionList()
    {
        if (missionListPanel != null)
            missionListPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    public void ToggleMissionList()
    {
        if (missionListPanel != null)
        {
            if (missionListPanel.activeInHierarchy)
                HideMissionList();
            else
                ShowMissionList();
        }
    }

    private void RefreshMissionList()
    {
        if (MissionManager.Instance?.allMissions == null) return;

        // Build mission strings for each category
        string mainMissionsText = "";
        string sideMissionsText = "";
        string completedMissionsText = "";

        foreach (var mission in MissionManager.Instance.allMissions)
        {
            string statusIcon = GetStatusIcon(mission.status);
            var sbLine = new StringBuilder();
            sbLine.Append(statusIcon).Append(' ').Append(mission.title);

            if (mission.status == MissionStatus.Active)
                sbLine.Append($" ({mission.currentStepIndex}/{mission.steps.Count})");

            sbLine.Append('\n');

            // Render per category
            if (mission.status == MissionStatus.Completed)
            {
                completedMissionsText += sbLine.ToString();
            }
            else if (mission.type == MissionType.Main)
            {
                mainMissionsText += sbLine.ToString();
                // ▼ Append completed steps + current step for ACTIVE missions
                if (mission.status == MissionStatus.Active)
                    mainMissionsText += BuildStepProgressBlock(mission);
            }
            else if (mission.type == MissionType.Side)
            {
                sideMissionsText += sbLine.ToString();
                if (mission.status == MissionStatus.Active)
                    sideMissionsText += BuildStepProgressBlock(mission);
            }
        }

        // Apply to UI
        UpdateSectionText(mainJobsParent, mainMissionsText);
        UpdateSectionText(sideJobsParent, sideMissionsText);
        UpdateSectionText(completedParent, completedMissionsText);
    }

    private string BuildStepProgressBlock(Mission mission)
    {
        if (mission.steps == null || mission.steps.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        // Completed steps
        for (int i = 0; i < mission.steps.Count; i++)
        {
            var step = mission.steps[i];
            if (step.isCompleted)
            {
                sb.Append(stepIndent).Append(completedStepIcon).Append(step.description).Append('\n');
            }
        }
        // Current step (if any)
        var current = mission.GetCurrentStep();
        if (current != null && !current.isCompleted)
        {
            sb.Append(stepIndent).Append(currentStepIcon).Append(current.description).Append('\n');
        }
        return sb.ToString();
    }

    private void UpdateSectionText(Transform parent, string content)
    {
        if (parent == null) return;
        TextMeshProUGUI[] textComponents = parent.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var textComp in textComponents)
        {
            if (textComp.name.Contains("Header") ||
                textComp.text == "MAIN JOBS" ||
                textComp.text == "SIDE JOBS" ||
                textComp.text == "COMPLETED")
                continue;

            textComp.text = content;
            break;
        }
    }

    private string GetStatusIcon(MissionStatus status)
    {
        switch (status)
        {
            case MissionStatus.Active: return "-> ";
            case MissionStatus.Completed: return "[OK] ";
            case MissionStatus.Locked: return "[LOCK] ";
            case MissionStatus.Failed: return "[FAIL] ";
            default: return "•";
        }
    }
}