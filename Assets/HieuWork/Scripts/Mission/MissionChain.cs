using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MissionChain
{
    public string levelName;
    public List<MissionStep> steps = new List<MissionStep>();
    public int currentStepIndex = 0;

    public MissionStep GetCurrentStep()
    {
        if (currentStepIndex < steps.Count)
            return steps[currentStepIndex];
        return null;
    }

    public void CompleteCurrentStep()
    {
        if (currentStepIndex < steps.Count)
        {
            steps[currentStepIndex].CompleteStep();
            currentStepIndex++;
        }
    }

    public bool IsFinished()
    {
        return currentStepIndex >= steps.Count;
    }
}
