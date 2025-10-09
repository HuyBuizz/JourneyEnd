using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem;
using Unity.Behavior;
using UnityEditor.Rendering.Canvas.ShaderGraph;

[RequireComponent(typeof(SelectionManager))]
public class RTSUnitCommander : MonoBehaviour
{
    private Camera cam;

    // Sentinel + cờ để lọc khoảng cách click
    private Vector3 oldRightClickPosition = new Vector3(float.PositiveInfinity, 0, 0);
    private bool hasPrevClick = false;

    public GameObject hitTargetClicked = null;
    private GameObject lastIssuedTarget = null; // để tránh phát lại lệnh y hệt

    private SelectionManager selectionManager;

    [Header("Command")]
    [SerializeField] private string command = "None";
    private string oldCommand = "None";

    // Sửa typo: unitContainer (vẫn serialize tiếp nếu đổi tên field)
    [SerializeField] private GameObject unitContainer;

    [SerializeField] private KeyCode hybridKey;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private LayerMask interactableMask = ~0;

    // Ngưỡng so khoảng cách click
    private const float CLICK_EPS = 0.01f;
    private const float CLICK_EPS_SQR = CLICK_EPS * CLICK_EPS;

    /// <summary>
    /// Phát lệnh + vị trí đích.
    /// </summary>
    public event Action<string, Vector3, GameObject> OnCommandIssued;
    public event Action<string, GameObject> OnTakeCommandIssued;
    public event System.Action OnForceStop;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null) Debug.LogWarning("[RTSUnitCommander] Camera.main is null.");

        selectionManager = GetComponent<SelectionManager>();
        if (selectionManager == null) Debug.LogWarning("[RTSUnitCommander] SelectionManager component not found.");

        // Tìm theo tên (tuỳ dự án có thể bỏ hoặc serialized thẳng trong Inspector)
        unitContainer = GameObject.Find("UnitContainer");
        if (unitContainer == null) Debug.LogWarning("[RTSUnitCommander] UnitContainer GameObject not found in scene.");
    }

    private void Update()
    {
        if (cam == null || selectionManager == null) return;

        // Cập nhật lệnh theo phím số
        CommandCased();

        // Bỏ qua nếu đang trên UI (giữ nguyên cách cũ của bạn)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Right click?
        bool rightClickDown =
            Input.GetMouseButtonDown(1) ||
            (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);

        if (!rightClickDown) return;

        hitTargetClicked = null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1500f, groundMask, QueryTriggerInteraction.Ignore))
        {
            // Lọc click trùng vị trí gần như nhau
            if (hasPrevClick)
            {
                if ((hit.point - oldRightClickPosition).sqrMagnitude <= CLICK_EPS_SQR)
                    return;
            }

            // Gán target
            if (Physics.Raycast(ray, out RaycastHit hitTarget, 1500f, interactableMask, QueryTriggerInteraction.Ignore))
            {
                if (hitTarget.collider != null)
                {
                    hitTargetClicked = hitTarget.collider.gameObject;

                }
                else
                {
                    hitTargetClicked = null;
                }
            }


            if (hitTargetClicked != null)
            {
                command = "Take";
            }
            else
            {
                command = "Move";
            }
            // Nếu chưa chọn command cụ thể, mặc định là Move

            // Chỉ reset BehaviorGraphAgent nếu lệnh/tham số thực sự đổi
            bool commandChanged = command != oldCommand;
            bool targetChanged = hitTargetClicked != lastIssuedTarget;
            bool positionChangedEnough = !hasPrevClick || (hit.point - oldRightClickPosition).sqrMagnitude > CLICK_EPS_SQR;

            if (commandChanged || targetChanged || positionChangedEnough)
            {
                foreach (GameObject unit in selectionManager.SelectedUnits)
                {
                    if (!unit) continue;
                    if (unit.TryGetComponent<BehaviorGraphAgent>(out var agent))
                    {
                        // Nếu SDK có API reset riêng thì dùng thay cho toggle enable
                        agent.enabled = false;
                        agent.enabled = true;
                    }
                }

                Debug.Log($"[RTSUnitCommander] Issue '{command}' to pos {hit.point} target={(hitTargetClicked ? hitTargetClicked.name : "null")}");

                // Phát lệnh
                OnForceStop?.Invoke();
                StartCoroutine(EmitCommandNextFrame(command, hit.point, hitTargetClicked));

                // Cập nhật “trạng thái trước đó”
                oldRightClickPosition = hit.point;
                hasPrevClick = true;
                oldCommand = command;
                lastIssuedTarget = hitTargetClicked;
            }
            else
            {
                // Bỏ log nếu không cần spam
                // Debug.Log("[RTSUnitCommander] Ignored duplicate command/params.");
            }
        }
    }

    private void CommandCased()
    {
        if (selectionManager.SelectedUnits.Count == 0) return;

        string newCommand = "None";

        // Hàng số trên: Alpha1..3
        if (Input.GetKeyDown(KeyCode.Alpha1)) newCommand = "Attack";
        else if (Input.GetKeyDown(KeyCode.Alpha2)) newCommand = "Rescue";
        else if (Input.GetKeyDown(KeyCode.Alpha3)) newCommand = "Spray";

        // Tùy chọn: hỗ trợ Keypad1..3
        if (Input.GetKeyDown(KeyCode.Keypad1)) newCommand = "Attack";
        else if (Input.GetKeyDown(KeyCode.Keypad2)) newCommand = "Rescue";
        else if (Input.GetKeyDown(KeyCode.Keypad3)) newCommand = "Spray";

        // Nếu không bấm phím mới, giữ nguyên lệnh cũ
        if (newCommand == "None") return;

        // Nếu bấm lại cùng một phím -> toggle về None
        if (newCommand == oldCommand)
        {
            command = "None";
            oldCommand = "None";
        }
        else
        {
            command = newCommand;
            oldCommand = newCommand;
        }
    }

    private System.Collections.IEnumerator EmitCommandNextFrame(string cmd, Vector3 pos, GameObject target)
    {
        yield return null;
        OnCommandIssued?.Invoke(cmd, pos, target);
    }

    private void GetHybridKey() { }
}
