using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    public bool spawnOnStart = false;
    [Header("Prefab to Spawn")]
    public GameObject prefab;       // Prefab sẽ spawn
    public float spawnInterval = 1f; // Khoảng thời gian giữa các lần spawn (giây)
    private float timer = 0f;
    public GameObject goalPoint; // Điểm đích cho AI

    void Update()
    {
        if (prefab == null) return;

        if (spawnOnStart)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                Spawn();
                timer = 0f;
            }
        }
    }

    private void Spawn()
    {
        Instantiate(prefab, transform.position, transform.rotation);
        prefab.GetComponent<SimpleNavMove>()?.SetTarget(goalPoint.transform);
    }
}
