using System;
using UnityEngine;

public enum WeatherType
{
    Sunny,
    Rainy,
}

public class WeatherManager : MonoBehaviour
{
    public WeatherType currentWeather;
    public GameObject rainPrefab;
    public TimeManager timeManager;

    public event Action<WeatherType> OnWeatherChanged;

    private float weatherTimer;
    public float weatherChangeInterval = 300f; // 5 phút in-game

    void Start()
    {
        ApplyWeather(currentWeather);
    }

    void Update()
    {
        // Tự động đổi thời tiết mỗi weatherChangeInterval phút in-game
        weatherTimer += Time.deltaTime * timeManager.timeScale;
        if (weatherTimer >= weatherChangeInterval)
        {
            weatherTimer = 0f;
            RandomizeWeather();
        }
    }

    void ApplyWeather(WeatherType type)
    {
        currentWeather = type;
        rainPrefab.SetActive(type == WeatherType.Rainy);
        OnWeatherChanged?.Invoke(type);
    }

    public void SetWeather(WeatherType type)
    {
        ApplyWeather(type);
    }

    void RandomizeWeather()
    {
        // Ví dụ: ban đêm dễ mưa hơn
        bool isNight = timeManager.hour < 6 || timeManager.hour > 18;
        float rainChance = isNight ? 0.5f : 0.2f;
        WeatherType newWeather =
            UnityEngine.Random.value < rainChance ? WeatherType.Rainy : WeatherType.Sunny;
        ApplyWeather(newWeather);
    }
}
