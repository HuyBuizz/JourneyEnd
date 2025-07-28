using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public float fakeSpeed = 0.5f; // tốc độ tăng giả lập

    private float displayedProgress = 0f;

    private void Start()
    {
        if (string.IsNullOrEmpty(SceneLoader.SceneToLoad))
        {
            Debug.LogError("SceneToLoad chưa được gán! Quay lại MenuScene để gán scene cần load.");
            return;
        }

        StartCoroutine(LoadLevelAsync(SceneLoader.SceneToLoad));
    }

    IEnumerator LoadLevelAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Tăng dần giá trị hiển thị (không để nhảy quá nhanh)
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, fakeSpeed * Time.deltaTime);

            progressBar.value = displayedProgress;
            progressText.text = (displayedProgress * 100f).ToString("F0") + "%";

            // Khi gần đạt 100% thật và hiển thị cũng đã đầy
            if (operation.progress >= 0.9f && displayedProgress >= 1f)
            {
                yield return new WaitForSeconds(0.3f); // hiệu ứng chờ mượt hơn
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
