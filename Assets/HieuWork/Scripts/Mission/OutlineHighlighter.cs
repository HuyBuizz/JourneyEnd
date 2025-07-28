using UnityEngine;

public class OutlineHighlighter : MonoBehaviour
{
    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    private void Update()
    {
        var currentStep = MissionManager.Instance?.GetCurrentStep();
        if (currentStep == null)
        {
            outline.enabled = false;
            return;
        }

        if ((currentStep.targetNPC == gameObject || 
             currentStep.targetObject == gameObject || 
             (currentStep.targetLocation != null && currentStep.targetLocation.gameObject == gameObject)))
        {
            outline.enabled = true;
        }
        else
        {
            outline.enabled = false;
        }
    }
}
