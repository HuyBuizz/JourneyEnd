using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public int hour = 6;
    public int minute = 0;
    public float timeScale = 60f; // 1 real second = 1 in-game minute

    private float timer;

    void Update()
    {
        timer += Time.deltaTime * timeScale;
        if (timer >= 60f)
        {
            minute++;
            timer = 0f;
            if (minute >= 60)
            {
                minute = 0;
                hour++;
                if (hour >= 24) hour = 0;
            }
        }
    }

    public string GetTimeString()
    {
        return hour.ToString("00") + ":" + minute.ToString("00");
    }
}

