using UnityEngine;

public class FireSpreadSystem : MonoBehaviour
{
    public GameObject platform;
    public GameObject houseHoldObjects;
    public GameObject fatherPoint;
    public GameObject point;
    public float margin = 1f;      // Khoảng cách từ mép platform
    public float platformPointDensity = 0.2f;     // Số điểm trên mỗi mét vuông
    public float objectPointDensity = 0.25f; // Số điểm trên mỗi mét vuông cho các đối tượng household

    // Các biến kiểm soát vùng random của point con
    public float childPointWidth = 2.5f;   // Độ rộng phân bổ ngang
    public float childPointHeightMin = 0.0f; // Độ cao tối thiểu phía trên father
    public float childPointHeightMax = 0.0f; // Độ cao tối đa phía trên father

    void Start()
    {
        PlacePointsOnAllPlatforms();
        PlacePointsOnHouseHoldObjects();
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

            int totalPoints = Mathf.Max(1, Mathf.RoundToInt(area * platformPointDensity));
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
                    }
                }
            }
        }
    }

    float CalculateMeshSurfaceArea(Mesh mesh)
    {
        float area = 0f;
        var tris = mesh.triangles;
        var verts = mesh.vertices;
        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = verts[tris[i + 0]];
            Vector3 v1 = verts[tris[i + 1]];
            Vector3 v2 = verts[tris[i + 2]];
            area += Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
        }
        return area;
    }

    void PlacePointsOnHouseHoldObjects()
    {
        if (houseHoldObjects == null || fatherPoint == null || point == null) return;

        foreach (Transform child in houseHoldObjects.transform)
        {
            MeshCollider meshCol = child.GetComponent<MeshCollider>();
            if (meshCol == null || meshCol.sharedMesh == null) continue;

            Vector3 center = child.position;
            GameObject father = Instantiate(fatherPoint, center, Quaternion.identity, child);
            Transform pointContainer = father.transform.Find("PointContainer");
            if (pointContainer == null)
            {
                Debug.LogWarning("PointContainer not found in fatherPoint prefab.");
                continue;
            }

            Mesh mesh = meshCol.sharedMesh;
            float area = CalculateMeshSurfaceArea(mesh);
            int numPoints = Mathf.Max(1, Mathf.RoundToInt(area * objectPointDensity));

            for (int i = 0; i < numPoints; i++)
            {
                Vector3 randomPoint = GetRandomPointOnMesh(mesh, child);
                Instantiate(point, randomPoint, Quaternion.identity, pointContainer);
            }
        }
    }

    // Random một điểm trên mesh surface (local -> world)
    Vector3 GetRandomPointOnMesh(Mesh mesh, Transform transform)
    {
        var tris = mesh.triangles;
        var verts = mesh.vertices;

        // Chọn random một tam giác
        int triIndex = Random.Range(0, tris.Length / 3) * 3;
        Vector3 v0 = verts[tris[triIndex + 0]];
        Vector3 v1 = verts[tris[triIndex + 1]];
        Vector3 v2 = verts[tris[triIndex + 2]];

        // Random barycentric coordinates
        float a = Random.value;
        float b = Random.value;
        if (a + b > 1f)
        {
            a = 1f - a;
            b = 1f - b;
        }
        float c = 1f - a - b;

        Vector3 localPoint = a * v0 + b * v1 + c * v2;
        return transform.TransformPoint(localPoint);
    }
}