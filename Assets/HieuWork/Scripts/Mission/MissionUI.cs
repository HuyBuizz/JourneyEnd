using UnityEngine;
using UnityEngine.UI;

public class MissionUI : MonoBehaviour
{
    public Text missionText;

    void Update()
    {
        var step = MissionManager.Instance?.GetCurrentStep();
        if (step != null && missionText != null)
            missionText.text = "🧯 Nhiệm vụ: " + step.description;
        else
            missionText.text = "🎉 Tất cả nhiệm vụ đã hoàn thành!";
    }
}
