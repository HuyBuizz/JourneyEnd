using UnityEngine;

public class OutlineHighlighter : MonoBehaviour
{
    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    private void Update()
    {
        if (MissionManager.Instance == null)
        {
            if (outline != null)
                outline.enabled = false;
            return;
        }

        var selectedMission = MissionManager.Instance.selectedMission;
        if (selectedMission == null || selectedMission.status != MissionStatus.Active)
        {
            if (outline != null)
                outline.enabled = false;
            return;
        }

        var currentStep = selectedMission.GetCurrentStep();
        if (currentStep == null)
        {
            if (outline != null)
                outline.enabled = false;
            return;
        }

        // Check if this object is the target of current step
        bool shouldHighlight = false;

        if (currentStep.targetNPC == gameObject)
        {
            shouldHighlight = true;
        }
        else if (currentStep.targetObject == gameObject)
        {
            shouldHighlight = true;
        }
        else if (currentStep.targetLocation != null &&
                 currentStep.targetLocation.gameObject == gameObject)
        {
            shouldHighlight = true;
        }

        if (outline != null)
            outline.enabled = shouldHighlight;
    }
}