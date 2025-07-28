using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public TimeManager timeManager;

    void Update()
    {
        float timePercent = (timeManager.hour * 60 + timeManager.minute) / 1440f; // 1440 phút 1 ngày
        sun.transform.rotation = Quaternion.Euler(new Vector3(timePercent * 360f - 90f, 170f, 0));
        sun.intensity = Mathf.Clamp01(Mathf.Cos(timePercent * Mathf.PI * 2f)); // Giảm sáng ban đêm
    }
}

