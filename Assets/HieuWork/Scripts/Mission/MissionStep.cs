using UnityEngine;
[System.Serializable]
public class MissionStep
{
    public string id;
    public string description;
    public MissionStepType type;
    public bool isCompleted;

    // Các dữ liệu đặc trưng cho từng loại nhiệm vụ
    public GameObject targetNPC;
    public Transform targetLocation;
    public GameObject targetObject;
    public int peopleToRescue;
    public float fireAmountToExtinguish;

    public void CompleteStep()
    {
        isCompleted = true;
        Debug.Log("✅ Hoàn thành bước: " + description);
    }
}
