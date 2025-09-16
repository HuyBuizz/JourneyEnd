using UnityEngine;
using System.Collections.Generic;

public class PathAnimator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject arrowPrefab;
    public int arrowCount = 15;
    public float spacing = 4.0f;
    public float speed = 8.0f;
    public float yOffset = 0.1f;

    private List<GameObject> arrows = new List<GameObject>();
    private List<Vector3> currentPath = new List<Vector3>();
    private float totalPathLength;
    private float startingDistance = 0;

    void Start()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow Prefab is not assigned in PathAnimator!");
            this.enabled = false;
            return;
        }
        for (int i = 0; i < arrowCount; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab, transform);
            arrow.SetActive(false);
            arrows.Add(arrow);
        }
    }

    public void SetPath(List<Vector3> newPath)
    {
        // Tắt tất cả các mũi tên cũ
        foreach (var arrow in arrows)
        {
            if(arrow != null) arrow.SetActive(false);
        }

        currentPath = newPath;
        totalPathLength = 0;
        startingDistance = 0; // Reset hiệu ứng chạy

        if (currentPath == null || currentPath.Count < 2)
        {
            return;
        }
        
        // Tính tổng chiều dài lộ trình
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            totalPathLength += Vector3.Distance(currentPath[i], currentPath[i + 1]);
        }
    }

    void Update()
    {
        if (currentPath == null || currentPath.Count < 2)
        {
            return;
        }

        // Cập nhật khoảng cách bắt đầu để tạo hiệu ứng "chảy"
        startingDistance += Time.deltaTime * speed;
        // Dùng modulo để loop lại hiệu ứng
        if (startingDistance > totalPathLength)
        {
            startingDistance = 0;
        }

        // Cập nhật từng mũi tên
        for (int i = 0; i < arrowCount; i++)
        {
            // Tính khoảng cách của mũi tên này dọc theo lộ trình
            float distance = startingDistance + i * spacing;

            // Nếu khoảng cách này nằm ngoài chiều dài lộ trình, tắt mũi tên
            if (distance > totalPathLength)
            {
                arrows[i].SetActive(false);
                continue;
            }

            arrows[i].SetActive(true);
            
            // Tìm vị trí và hướng trên lộ trình
            (Vector3 position, Quaternion rotation) = GetPointAndRotationOnPath(distance);
            
            arrows[i].transform.position = position + Vector3.up * yOffset;
            arrows[i].transform.rotation = rotation;
        }
    }

    // Hàm phụ để tìm vị trí và góc xoay tại một khoảng cách nhất định
    private (Vector3, Quaternion) GetPointAndRotationOnPath(float distance)
    {
        float traveledDistance = 0;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 startPoint = currentPath[i];
            Vector3 endPoint = currentPath[i + 1];
            float segmentLength = Vector3.Distance(startPoint, endPoint);

            if (traveledDistance + segmentLength >= distance)
            {
                float distanceOnSegment = distance - traveledDistance;
                Vector3 position = Vector3.Lerp(startPoint, endPoint, distanceOnSegment / segmentLength);
                Quaternion rotation = Quaternion.LookRotation(endPoint - startPoint);
                return (position, rotation);
            }
            traveledDistance += segmentLength;
        }

        // Nếu đã ở cuối hoặc có lỗi, trả về điểm cuối
        Vector3 finalPos = currentPath[currentPath.Count - 1];
        Quaternion finalRot = currentPath.Count > 1 ? Quaternion.LookRotation(finalPos - currentPath[currentPath.Count - 2]) : Quaternion.identity;
        return (finalPos, finalRot);
    }
}