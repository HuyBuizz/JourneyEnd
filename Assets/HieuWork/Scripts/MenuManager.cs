using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnStartGame()
    {
        SceneLoader.SceneToLoad = "Level1"; // ← Lưu tên scene cần load
        SceneManager.LoadScene("Loading"); // ← Sang scene Loading
    }
}
