using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    [Header("Objective Panel - Small UI")]
    public GameObject objectivePanel;
    public TextMeshProUGUI objectiveTitleText;
    public TextMeshProUGUI objectiveDescriptionText;
    public TextMeshProUGUI stepDescriptionText;
    public Slider progressSlider;

    [Header("Controls")]
    public KeyCode openMissionListKey = KeyCode.Tab;

    private MissionListUI missionListUI;

    private void Start()
    {
        missionListUI = FindFirstObjectByType<MissionListUI>();

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnSelectedMissionChanged += UpdateObjectiveDisplay;
            MissionManager.Instance.OnMissionStepChanged += UpdateObjectiveDisplay;
        }

        if (objectivePanel != null)
            objectivePanel.SetActive(true);

        Invoke("UpdateObjectiveDisplay", 0.2f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(openMissionListKey))
        {
            missionListUI?.ToggleMissionList();
        }
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnSelectedMissionChanged -= UpdateObjectiveDisplay;
            MissionManager.Instance.OnMissionStepChanged -= UpdateObjectiveDisplay;
        }
    }

    private void UpdateObjectiveDisplay()
    {
        if (MissionManager.Instance == null)
        {
            SetDefaultDisplay();
            return;
        }

        var selectedMission = MissionManager.Instance.selectedMission;

        if (selectedMission != null && selectedMission.status == MissionStatus.Active)
        {
            if (objectiveDescriptionText != null)
                objectiveDescriptionText.text = selectedMission.title;

            var currentStep = selectedMission.GetCurrentStep();
            if (currentStep != null && stepDescriptionText != null)
            {
                stepDescriptionText.text = currentStep.description;
            }
            else if (stepDescriptionText != null)
            {
                stepDescriptionText.text = "Mission Complete!";
            }

            if (progressSlider != null)
            {
                progressSlider.value = selectedMission.GetProgress();
                progressSlider.gameObject.SetActive(selectedMission.steps.Count > 1);
            }
        }
        else
        {
            SetDefaultDisplay();
        }
    }

    private void SetDefaultDisplay()
    {
        if (objectiveDescriptionText != null)
            objectiveDescriptionText.text = "No Active Mission";

        if (stepDescriptionText != null)
            stepDescriptionText.text = "All missions completed!";

        if (progressSlider != null)
            progressSlider.gameObject.SetActive(false);
    }
}