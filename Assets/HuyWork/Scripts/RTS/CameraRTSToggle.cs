using StarterAssets;
using UnityEngine;

public class CameraRTSToggle : MonoBehaviour
{
    [Header("GOplayer refs")]
    public GameObject GOPlayer;
    private GameObject playerCameraRoot;
    private StarterAssetsInputs starterInputs;

    [Header("Phím chuyển chế độ")]
    public KeyCode toggleKey = KeyCode.C;

    [Header("Thiết lập góc nhìn RTS")]
    public Vector3 rtsPositionAdjust = new Vector3(0, 0, 0);
    public Vector3 rtsRotation = new Vector3(40, 0, 0);

    [Header("Điều khiển RTS")]
    public float moveSpeed = 30f;
    public float zoomSpeed = 200f;
    public float rotateSpeed = 70f;
    public float minZoom = 20f;
    public float maxZoom = 100f;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isRTS = false;

    void Start()
    {
        if (playerCameraRoot == null)
        {
            playerCameraRoot = GOPlayer.transform.Find("PlayerCameraRoot").gameObject;
        }

        if (starterInputs == null)
        {
            starterInputs = GOPlayer.GetComponent<StarterAssetsInputs>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleRTSCameraMode();
        }

        if (isRTS)
        {
            HandleRTSMovement();
        }
        else
        {
            CameraOriginalTransform();
        }
    }

    void CameraOriginalTransform()
    {
        originalPosition = playerCameraRoot.transform.position;
        originalRotation = playerCameraRoot.transform.rotation;
    }

    void ToggleRTSCameraMode()
    {
        if (!isRTS)
        {
            // Bật RTS mode
            DisableAllGOPlayerComponents(GOPlayer);
            playerCameraRoot.transform.position = originalPosition + rtsPositionAdjust;
            playerCameraRoot.transform.rotation = Quaternion.Euler(rtsRotation);

            if (starterInputs != null)
            {
                starterInputs.cursorLocked = false;
                starterInputs.cursorInputForLook = false;
                Cursor.lockState = CursorLockMode.None; // unlock luôn cho chắc
            }
        }
        else
        {
            // Trả lại FPS mode
            EnableAllGOPlayerComponents(GOPlayer);
            playerCameraRoot.transform.position = originalPosition;
            playerCameraRoot.transform.rotation = originalRotation;

            if (starterInputs != null)
            {
                starterInputs.cursorLocked = true;
                starterInputs.cursorInputForLook = true;
                Cursor.lockState = CursorLockMode.Locked; // lock lại
            }
        }

        isRTS = !isRTS;
    }

    void HandleRTSMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = (playerCameraRoot.transform.forward * v + playerCameraRoot.transform.right * h);
        moveDir.y = 0;
        playerCameraRoot.transform.position += moveDir * moveSpeed * Time.deltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = playerCameraRoot.transform.position;
        pos += playerCameraRoot.transform.forward * scroll * zoomSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);
        playerCameraRoot.transform.position = pos;

        if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            float mouseX = Input.GetAxis("Mouse X");
            playerCameraRoot.transform.Rotate(Vector3.up, mouseX * rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    void DisableAllGOPlayerComponents(GameObject gameObject)
    {
        // Tắt tất cả Behaviour (MonoBehaviour, NavMeshAgent, Animator,...)
        foreach (var b in gameObject.GetComponents<Behaviour>())
        {
            if (b is StarterAssetsInputs)
                continue; // Giữ lại 2 component này để tránh lỗi
            b.enabled = false;
        }

        // Tắt các Renderer (Renderer không kế thừa Behaviour)
        foreach (var r in gameObject.GetComponents<Renderer>())
            r.enabled = false;

        // Tắt Collider (nhiều Collider không kế thừa Behaviour)
        foreach (var c in gameObject.GetComponents<Collider>())
            c.enabled = false;
    }

    void EnableAllGOPlayerComponents(GameObject gameObject)
    {
        // Bật tất cả Behaviour (MonoBehaviour, NavMeshAgent, Animator,...)
        foreach (var b in gameObject.GetComponents<Behaviour>())
            b.enabled = true;

        // Bật các Renderer (Renderer không kế thừa Behaviour)
        foreach (var r in gameObject.GetComponents<Renderer>())
            r.enabled = true;

        // Bật Collider (nhiều Collider không kế thừa Behaviour)
        foreach (var c in gameObject.GetComponents<Collider>())
            c.enabled = true;
    }
}

