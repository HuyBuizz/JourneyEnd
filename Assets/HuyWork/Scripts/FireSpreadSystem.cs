using UnityEngine;

public class FireSpreadSystem : MonoBehaviour
{
    public GameObject platform;
    public GameObject fatherPoint;
    public GameObject point;
    public float margin = 0.5f;      // Khoảng cách từ mép platform
    public float density = 1.0f;     // Số điểm trên mỗi mét vuông

    // Các biến kiểm soát vùng random của point con
    public float childPointWidth = 0.4f;   // Độ rộng phân bổ ngang
    public float childPointHeightMin = 0.2f; // Độ cao tối thiểu phía trên father
    public float childPointHeightMax = 0.5f; // Độ cao tối đa phía trên father

    void Start()
    {
        PlacePointsOnAllPlatforms();
    }

    void PlacePointsOnAllPlatforms()
    {
        if (platform == null || fatherPoint == null || point == null) return;

        foreach (Transform child in platform.transform)
        {
            BoxCollider col = child.GetComponent<BoxCollider>();
            if (col == null) continue;

            Vector3 size = Vector3.Scale(col.size, child.localScale);
            Vector3 center = child.position;

            float usableX = size.x - 2 * margin;
            float usableZ = size.z - 2 * margin;
            float area = usableX * usableZ;

            int totalPoints = Mathf.Max(1, Mathf.RoundToInt(area * density));
            int pointsPerRow = Mathf.Max(2, Mathf.RoundToInt(Mathf.Sqrt(totalPoints)));

            float stepX = usableX / (pointsPerRow - 1);
            float stepZ = usableZ / (pointsPerRow - 1);
            float y = center.y + size.y / 2;

            for (int i = 0; i < pointsPerRow; i++)
            {
                for (int j = 0; j < pointsPerRow; j++)
                {
                    float x = center.x - size.x / 2 + margin + i * stepX;
                    float z = center.z - size.z / 2 + margin + j * stepZ;
                    Vector3 fatherPos = new Vector3(x, y, z);
                    GameObject father = Instantiate(fatherPoint, fatherPos, Quaternion.identity, child);

                    // Đặt 3 point con ở vị trí random phía trên father point, kiểm soát vùng random
                    for (int k = 0; k < 3; k++)
                    {
                        float randX = Random.Range(-childPointWidth / 2f, childPointWidth / 2f);
                        float randZ = Random.Range(-childPointWidth / 2f, childPointWidth / 2f);
                        float randY = Random.Range(childPointHeightMin, childPointHeightMax);
                        Vector3 childPos = fatherPos + new Vector3(randX, randY, randZ);
                        Instantiate(point, childPos, Quaternion.identity, father.transform.Find("PointContainer"));
                        point.SetActive(false);
                    }
                }
            }
        }
    }
}