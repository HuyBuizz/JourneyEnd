using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public TimeManager timeManager;
    public WeatherManager weatherManager;

    private float rainLightMultiplier = 0.6f;

    void Start()
    {
        if (weatherManager != null)
            weatherManager.OnWeatherChanged += OnWeatherChanged;
    }

    void Update()
    {
        float timePercent = (timeManager.hour * 60 + timeManager.minute) / 1440f; // 1440 phút 1 ngày
        sun.transform.rotation = Quaternion.Euler(new Vector3(timePercent * 360f - 90f, 170f, 0));
        float baseIntensity = Mathf.Clamp01(Mathf.Cos(timePercent * Mathf.PI * 2f)); // Giảm sáng ban đêm
        sun.intensity =
            baseIntensity
            * (
                weatherManager != null && weatherManager.currentWeather == WeatherType.Rainy
                    ? rainLightMultiplier
                    : 1f
            );
    }

    void OnWeatherChanged(WeatherType type)
    {
        // Có thể thêm hiệu ứng chuyển đổi ánh sáng mượt hơn ở đây
    }
}
