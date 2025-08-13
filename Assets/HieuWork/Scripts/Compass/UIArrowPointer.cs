using TMPro; // Thêm dòng này
using UnityEngine;
using UnityEngine.UI;

public class UIArrowPointer : MonoBehaviour
{
    public Transform player; // Player position
    public Transform target; // Target position
    public Camera playerCamera; // Player's camera
    public RectTransform arrowUI; // UI arrow image
    public TMP_Text distanceText; // TMP để hiển thị khoảng cách

    void Start()
    {
        // Đăng ký sự kiện khi nhiệm vụ đổi
        MissionManager.Instance.OnMissionStepChanged += UpdateTarget;
        // Gọi lần đầu
        UpdateTarget();
    }

    void OnDestroy()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionStepChanged -= UpdateTarget;
    }

    void UpdateTarget()
    {
        target = MissionManager.Instance.GetCurrentTarget();
    }

    void Update()
    {
        if (target == null || player == null)
        {
            if (distanceText != null)
                distanceText.text = "";
            return;
        }

        // Vector từ player đến target
        Vector3 dirToTarget = target.position - player.position;
        // Vẽ line giữa player và target (màu đỏ, hiển thị trong Scene view)
        Debug.DrawLine(player.position, target.position, Color.red);

        // Chuyển vector sang tọa độ local theo hướng camera
        Vector3 dirLocal = playerCamera.transform.InverseTransformDirection(dirToTarget);

        // Tính góc xoay trên mặt phẳng XZ
        float angle = Mathf.Atan2(dirLocal.x, dirLocal.z) * Mathf.Rad2Deg;

        // Gán góc xoay cho mũi tên UI
        arrowUI.localEulerAngles = new Vector3(0, 0, -angle + 90f);

        // Hiển thị khoảng cách
        if (distanceText != null)
        {
            float distance = dirToTarget.magnitude;
            distanceText.text = $"{distance:F1} m";
        }
    }
}
