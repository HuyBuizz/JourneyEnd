using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Tạo xe thang cứu hỏa - Thang Telescoping THẬT
/// Unity 6.2 - 02/10/2025
/// Nhiều đoạn thang nhỏ nối liền, đẩy ra khi kéo dài
/// </summary>
public class FireTruckSetupHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/🚒 Tạo Xe Thang Cứu Hỏa", false, 0)]
    public static void CreateFireTruckScene()
    {
        // Xóa scene cũ
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name != "Main Camera" && obj.name != "Directional Light")
            {
                DestroyImmediate(obj);
            }
        }

        CreateGround();
        GameObject truck = CreateFireTruck();
        SetupCamera();

        Debug.Log("✅ XE THANG - Thang telescoping THẬT!");

        Selection.activeGameObject = truck;
    }

    static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.3f, 0.3f, 0.3f);
        ground.GetComponent<Renderer>().material = mat;
    }

    static GameObject CreateFireTruck()
    {
        GameObject truck = new GameObject("FireTruck");
        truck.transform.position = new Vector3(0f, 0.5f, 0f);

        // Thân xe
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(truck.transform);
        body.transform.localPosition = new Vector3(0f, 0f, 0f);
        body.transform.localScale = new Vector3(2.5f, 1.5f, 5f);

        Material redMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        redMat.color = new Color(0.9f, 0.1f, 0.1f);
        body.GetComponent<Renderer>().material = redMat;

        // Cabin
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(truck.transform);
        cabin.transform.localPosition = new Vector3(0f, 0.8f, 2f);
        cabin.transform.localScale = new Vector3(2.2f, 1.5f, 1.8f);

        Material whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        whiteMat.color = new Color(1f, 1f, 1f);
        cabin.GetComponent<Renderer>().material = whiteMat;

        // Hệ thống thang
        GameObject ladderSystem = CreateLadderSystem();
        ladderSystem.transform.SetParent(truck.transform);
        ladderSystem.transform.localPosition = new Vector3(0f, 1.2f, -1f);

        // Controller
        FireTruckLadderController controller = truck.AddComponent<FireTruckLadderController>();
        SetupController(controller, ladderSystem);

        return truck;
    }

    static GameObject CreateLadderSystem()
    {
        GameObject system = new GameObject("LadderSystem");

        // Đế thang
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = "LadderBase";
        baseObj.transform.SetParent(system.transform);
        baseObj.transform.localPosition = Vector3.zero;
        baseObj.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);

        Material baseMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        baseMat.color = new Color(0.2f, 0.2f, 0.2f);
        baseObj.GetComponent<Renderer>().material = baseMat;

        // Cánh tay thang
        GameObject arm = new GameObject("LadderArm");
        arm.transform.SetParent(baseObj.transform);
        arm.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        arm.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);

        // Bracket
        GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bracket.name = "Bracket";
        bracket.transform.SetParent(arm.transform);
        bracket.transform.localPosition = Vector3.zero;
        bracket.transform.localScale = new Vector3(0.5f, 0.5f, 0.8f);
        bracket.GetComponent<Renderer>().material = baseMat;

        Material ladderMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ladderMat.color = new Color(0.9f, 0.85f, 0.3f);

        // Container cho các segments thang
        GameObject ladderContainer = new GameObject("Ladder");
        ladderContainer.transform.SetParent(arm.transform);
        ladderContainer.transform.localPosition = Vector3.zero;

        // Tạo 10 segments thang (mỗi segment = 2 units)
        float segmentLength = 2f;
        for (int i = 0; i < 10; i++)
        {
            GameObject segment = CreateLadderSegment($"Segment{i}", 0.3f, segmentLength, ladderMat);
            segment.transform.SetParent(ladderContainer.transform);
            // Tất cả segments bắt đầu ở vị trí gốc (chồng lên nhau)
            segment.transform.localPosition = new Vector3(0f, 0f, segmentLength * 0.5f);
        }

        return system;
    }

    /// <summary>
    /// Tạo 1 segment thang - có bậc thang cố định
    /// </summary>
    static GameObject CreateLadderSegment(string name, float width, float length, Material mat)
    {
        GameObject segment = new GameObject(name);

        // Thanh trái
        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        left.name = "Left";
        left.transform.SetParent(segment.transform);
        left.transform.localPosition = new Vector3(-width, 0f, 0f);
        left.transform.localScale = new Vector3(0.08f, 0.08f, length);
        left.GetComponent<Renderer>().material = mat;

        // Thanh phải
        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        right.name = "Right";
        right.transform.SetParent(segment.transform);
        right.transform.localPosition = new Vector3(width, 0f, 0f);
        right.transform.localScale = new Vector3(0.08f, 0.08f, length);
        right.GetComponent<Renderer>().material = mat;

        // Bậc thang (rungs) - CỐ ĐỊNH
        int numRungs = 4; // 4 bậc cho mỗi segment 2 units
        float rungSpacing = length / (numRungs + 1);
        float startZ = -(length / 2f) + rungSpacing;

        for (int i = 0; i < numRungs; i++)
        {
            GameObject rung = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rung.name = $"Rung{i}";
            rung.transform.SetParent(segment.transform);
            rung.transform.localPosition = new Vector3(0f, 0f, startZ + i * rungSpacing);
            rung.transform.localScale = new Vector3(width * 2.5f, 0.06f, 0.06f);
            rung.GetComponent<Renderer>().material = mat;
        }

        return segment;
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(-8f, 8f, 8f);
            cam.transform.LookAt(new Vector3(0f, 5f, 0f));
        }
    }

    static void SetupController(FireTruckLadderController controller, GameObject ladderSystem)
    {
        Transform baseTransform = ladderSystem.transform.Find("LadderBase");
        Transform armTransform = baseTransform.Find("LadderArm");
        Transform ladderContainer = armTransform.Find("Ladder");

        // Lấy tất cả segments
        Transform[] segments = new Transform[ladderContainer.childCount];
        for (int i = 0; i < ladderContainer.childCount; i++)
        {
            segments[i] = ladderContainer.GetChild(i);
        }

        var field1 = controller.GetType().GetField("ladderBase",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field1?.SetValue(controller, baseTransform);

        var field2 = controller.GetType().GetField("ladderArm",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field2?.SetValue(controller, armTransform);

        var field3 = controller.GetType().GetField("ladderSegments",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field3?.SetValue(controller, segments);
    }
#endif
}