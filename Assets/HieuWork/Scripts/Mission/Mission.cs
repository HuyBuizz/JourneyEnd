using System.Collections.Generic;


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
public class Mission
{
    public string id;
    public string title;
    public string description;
    public MissionType type;
    public MissionStatus status = MissionStatus.Locked;
    public List<MissionStep> steps = new List<MissionStep>();
    public int currentStepIndex = 0;
    // Quan hệ cha - con (Main chứa các Side con)
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


    public void CompleteCurrentStep()
    {
        if (currentStepIndex < steps.Count)
        {
            if (!steps[currentStepIndex].isCompleted)
                steps[currentStepIndex].CompleteStep();


            currentStepIndex++;
            TryMarkCompleted();
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