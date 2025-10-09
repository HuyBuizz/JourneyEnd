using UnityEngine;

/// <summary>
/// Điều khiển thang xe cứu hỏa - ĐẨY SEGMENTS RA
/// Unity 6.2 - 02/10/2025
/// Không scale, chỉ di chuyển segments
/// </summary>
public class FireTruckLadderController : MonoBehaviour
{
    [Header("⚙️ Components")]
    [SerializeField] private Transform ladderBase;
    [SerializeField] private Transform ladderArm;
    [SerializeField] private Transform[] ladderSegments;

    [Header("🔄 Rotation")]
    [SerializeField] private float horizontalSpeed = 30f;
    [SerializeField] private float verticalSpeed = 20f;
    [SerializeField] private float minVerticalAngle = 30f;
    [SerializeField] private float maxVerticalAngle = 90f;

    [Header("📏 Extension")]
    [SerializeField] private float extensionSpeed = 1.5f;
    [SerializeField] private float segmentLength = 2f; // Chiều dài mỗi segment

    [Header("🎮 Controls")]
    [SerializeField] private KeyCode tiltUp = KeyCode.Keypad8;
    [SerializeField] private KeyCode tiltDown = KeyCode.Keypad2;
    [SerializeField] private KeyCode rotateLeft = KeyCode.Keypad4;
    [SerializeField] private KeyCode rotateRight = KeyCode.Keypad6;
    [SerializeField] private KeyCode extend = KeyCode.UpArrow;
    [SerializeField] private KeyCode retract = KeyCode.DownArrow;

    // State
    private float currentExtension = 0f; // 0 = thu vào, 1 = kéo tối đa
    private float currentVerticalAngle = 60f;
    private float currentHorizontalAngle = 0f;

    void Start()
    {
        if (ladderBase != null)
            currentHorizontalAngle = ladderBase.localEulerAngles.y;

        if (ladderArm != null)
            currentVerticalAngle = ladderArm.localEulerAngles.x;

        // Bắt đầu thu gọn
        currentExtension = 0f;
        UpdateLadderSegments();

        Debug.Log("🚒 XE THANG - Đẩy segments ra, KHÔNG scale!");
    }

    void Update()
    {
        HandleTilt();
        HandleRotation();
        HandleExtension();
    }

    void HandleTilt()
    {
        float input = 0f;
        if (Input.GetKey(tiltUp)) input = 1f;
        else if (Input.GetKey(tiltDown)) input = -1f;

        if (Mathf.Abs(input) > 0.01f && ladderArm != null)
        {
            currentVerticalAngle += input * verticalSpeed * Time.deltaTime;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
            ladderArm.localRotation = Quaternion.Euler(currentVerticalAngle, 0f, 0f);
        }
    }

    void HandleRotation()
    {
        float input = 0f;
        if (Input.GetKey(rotateRight)) input = 1f;
        else if (Input.GetKey(rotateLeft)) input = -1f;

        if (Mathf.Abs(input) > 0.01f && ladderBase != null)
        {
            currentHorizontalAngle += input * horizontalSpeed * Time.deltaTime;
            ladderBase.localRotation = Quaternion.Euler(0f, currentHorizontalAngle, 0f);
        }
    }

    void HandleExtension()
    {
        float input = 0f;
        if (Input.GetKey(extend)) input = 1f;
        else if (Input.GetKey(retract)) input = -1f;

        if (Mathf.Abs(input) > 0.01f)
        {
            currentExtension += input * extensionSpeed * Time.deltaTime;
            currentExtension = Mathf.Clamp01(currentExtension);
            UpdateLadderSegments();
        }
    }

    /// <summary>
    /// CẬP NHẬT VỊ TRÍ CÁC SEGMENTS - ĐẨY RA TỪNG ĐOẠN
    /// </summary>
    void UpdateLadderSegments()
    {
        if (ladderSegments == null || ladderSegments.Length == 0) return;

        int numSegments = ladderSegments.Length;

        // Tính tổng chiều dài kéo ra
        float totalExtension = currentExtension * (numSegments * segmentLength);

        for (int i = 0; i < numSegments; i++)
        {
            if (ladderSegments[i] == null) continue;

            // Segment i đẩy ra = (i / numSegments) * totalExtension
            float segmentOffset = (float)i / numSegments * totalExtension;

            Vector3 newPos = ladderSegments[i].localPosition;
            newPos.z = (segmentLength * 0.5f) + segmentOffset; // Gốc + offset
            ladderSegments[i].localPosition = newPos;
        }
    }

    void OnDrawGizmos()
    {
        if (ladderSegments == null || ladderSegments.Length == 0) return;

        // Vẽ từng segment
        for (int i = 0; i < ladderSegments.Length; i++)
        {
            if (ladderSegments[i] != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
                Vector3 start = ladderSegments[i].position - ladderSegments[i].forward * (segmentLength * 0.5f);
                Vector3 end = ladderSegments[i].position + ladderSegments[i].forward * (segmentLength * 0.5f);
                Gizmos.DrawLine(start, end);
            }
        }

        // Vẽ đỉnh thang
        if (ladderSegments.Length > 0 && ladderSegments[ladderSegments.Length - 1] != null)
        {
            Transform lastSegment = ladderSegments[ladderSegments.Length - 1];
            Vector3 tipPos = lastSegment.position + lastSegment.forward * (segmentLength * 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(tipPos, 0.5f);
        }
    }
}