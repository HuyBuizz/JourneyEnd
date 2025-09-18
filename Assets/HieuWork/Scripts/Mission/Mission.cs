using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum MissionType
{
    Main,
    Side
}


[System.Serializable]
public enum MissionStatus
{
    Locked,
    Active,
    Completed,
    Failed
}

[System.Serializable]
public enum PlayerRole
{
    None,
    Firefighter,
    Medic,
    Engineer,
    Commander
}

[System.Serializable]
public class Mission
{
    public string id;
    public string title;
    public string description;
    public MissionType type;
    public MissionStatus status = MissionStatus.Locked;
    public PlayerRole assignedRole = PlayerRole.Firefighter;
    [SerializeReference]
    public List<MissionStep> steps = new List<MissionStep>();
    public int currentStepIndex = 0;

    // Quan hệ cha - con (Main chứa các Side con)
    [SerializeReference]
    public List<Mission> children = new List<Mission>();
    public string parentId; // null nếu là root


    // Điều kiện để unlock child dựa trên tiến độ của parent
    public int unlockAfterParentStep = -1; // -1 = mở ngay khi parent Active


    // Đánh dấu child này có bắt buộc để parent được Completed hay không
    public bool isRequiredChild = false;


    // (Giữ để tương thích — có thể dùng cho dependency theo id ngoài cây)
    public List<string> requiredCompletedMissions = new List<string>();



    public MissionStep GetCurrentStep()
    {
        if (status != MissionStatus.Active) return null;
        if (currentStepIndex < 0 || currentStepIndex >= steps.Count) return null;
        return steps[currentStepIndex];
    }

    public void InitializeSteps()
    {
        if (steps == null || steps.Count == 0) return;

        currentStepIndex = 0;

        for (int i = 0; i < steps.Count; i++)
        {
            steps[i].isCompleted = false;
            steps[i].isActive = (i == 0); // chỉ mở step 1
        }
    }


    public void CompleteCurrentStep()
    {
        if (status != MissionStatus.Active) return;

        var step = GetCurrentStep();
        if (step != null)
        {
            step.CompleteStep();

            // Sang step tiếp theo
            currentStepIndex++;

            if (currentStepIndex < steps.Count)
            {
                steps[currentStepIndex].isActive = true;
                Debug.Log($"[Mission] Step {steps[currentStepIndex].id} is now active");
            }
            else
            {
                status = MissionStatus.Completed;
                Debug.Log($"[Mission] Mission {id} completed!");
            }
        }
    }


    public bool StepsCompleted()
    {
        return currentStepIndex >= steps.Count;
    }


    public bool AreRequiredChildrenCompleted()
    {
        for (int i = 0; i < children.Count; i++)
        {
            var c = children[i];
            if (c.isRequiredChild && c.status != MissionStatus.Completed)
                return false;
        }
        return true;
    }


    public bool IsCompleted()
    {
        return StepsCompleted() && AreRequiredChildrenCompleted();
    }


    public void TryMarkCompleted()
    {
        if (IsCompleted())
        {
            status = MissionStatus.Completed;
        }
    }


    public float GetProgress()
    {
        if (steps == null || steps.Count == 0) return 0f;
        return (float)currentStepIndex / steps.Count;
    }
}