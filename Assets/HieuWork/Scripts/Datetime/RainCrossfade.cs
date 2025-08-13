using UnityEngine;

public class RainCrossfade : MonoBehaviour
{
    public AudioSource sourceA;
    public AudioSource sourceB;
    public float fadeDuration = 2f; // Thời gian fade-in/out
    private AudioSource currentSource;
    private AudioSource nextSource;
    private float clipLength;

    public GameObject rainPrefab;

    void Start()
    {
        // Lấy độ dài clip
        clipLength = sourceA.clip.length;

        // Đặt nguồn phát đầu tiên
        currentSource = sourceA;
        nextSource = sourceB;

        // Phát nguồn đầu tiên
        currentSource.volume = 1f;
        currentSource.Play();

        // Bắt đầu vòng lặp crossfade
        InvokeRepeating(
            nameof(ScheduleNextFade),
            clipLength - fadeDuration,
            clipLength - fadeDuration
        );
    }

    void ScheduleNextFade()
    {
        if (rainPrefab.activeSelf)
        {
            // Chuẩn bị nguồn kế tiếp
            nextSource.volume = 0f;
            nextSource.Play();

            // Bắt đầu fade
            StartCoroutine(FadeAudio(currentSource, nextSource));
        }
        else
        {
            // Nếu mưa tắt, dừng cả hai AudioSource
            currentSource.Stop();
            nextSource.Stop();
        }

        // Đổi nguồn cho lần tiếp theo
        var temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
    }

    System.Collections.IEnumerator FadeAudio(AudioSource from, AudioSource to)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            from.volume = Mathf.Lerp(1f, 0f, t);
            to.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        from.Stop();
    }
}
