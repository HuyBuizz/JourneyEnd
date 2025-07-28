using UnityEngine;
public enum WeatherType { Sunny, Rainy }

public class WeatherManager : MonoBehaviour
{
    public WeatherType currentWeather;
    public GameObject rainPrefab;

    void Update()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny:
                rainPrefab.SetActive(false);
                break;
            case WeatherType.Rainy:
                rainPrefab.SetActive(true);
                break;
        }
    }

    public void SetWeather(WeatherType type)
    {
        currentWeather = type;
    }
}

