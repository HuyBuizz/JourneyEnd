using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject prefab;       // Prefab sẽ spawn
    public Transform spawnPoint;    // GameObject để xác định vị trí spawn
    public int spawnCount = 1;      // Số lượng spawn
    public float spawnInterval = 1f; // Khoảng thời gian giữa các lần spawn (giây)

    private float timer = 0f;
    private int spawned = 0;

    void Update()
    {
        if (prefab == null || spawnPoint == null) return;

        if (spawned < spawnCount)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                Spawn();
                timer = 0f;
                spawned++;
            }
        }
    }

    private void Spawn()
    {
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
