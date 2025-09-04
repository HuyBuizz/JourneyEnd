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


    // Tiến độ (nếu là dạng tích lũy)
    public int peopleToRescue; // ngưỡng cần cứu
    [HideInInspector] public int rescuedCount; // đã cứu


    public float fireAmountToExtinguish; // ngưỡng dập lửa
    [HideInInspector] public float extinguishedAmount; // đã dập


    public void CompleteStep()
    {
        isCompleted = true;
        Debug.Log("✅ Hoàn thành bước: " + description);
    }


    public MissionStep Clone()
    {
        return new MissionStep
        {
            id = this.id,
            description = this.description,
            type = this.type,
            isCompleted = this.isCompleted,
            targetNPC = this.targetNPC,
            targetLocation = this.targetLocation,
            targetObject = this.targetObject,
            peopleToRescue = this.peopleToRescue,
            rescuedCount = this.rescuedCount,
            fireAmountToExtinguish = this.fireAmountToExtinguish,
            extinguishedAmount = this.extinguishedAmount
        };
    }
}