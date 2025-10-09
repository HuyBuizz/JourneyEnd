using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameObject playerCameraRoot;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerAction playerAction;
    [SerializeField] PlayerState playerState;
    [SerializeField] MultiKeyHintUI keyHintUI;


    [Header("Config")]
    [SerializeField] LayerMask interactableMask;
    [SerializeField] float raycastInterval = 0.02f;
    [SerializeField] GameObject interactableObject;
    GameObject lastOutlinedObject;
    Outline lastOutline;
    InputAction interactAction;
    float _rayTimer;

    void Awake()
    {
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
        if (!playerAction) playerAction = GetComponent<PlayerAction>();
        if (!playerState) playerState = GetComponent<PlayerState>();

        interactAction = playerInput.actions["Interact"];
    }

    void Start()
    {
        if (!playerCameraRoot)
        {
            var fpc = GetComponent<FirstPersonController>();
            if (fpc != null) playerCameraRoot = fpc.CinemachineCameraTarget;
        }
    }

    void OnEnable()
    {
        interactAction?.Enable();
    }

    void OnDisable()
    {
        interactAction?.Disable();

        // tắt outline khi disable
        if (lastOutline) lastOutline.enabled = false;
        lastOutline = null;
        lastOutlinedObject = null;
    }

    void Update()
    {
        // Detect + outline không cần 60–144 Hz, giảm tải nhẹ
        _rayTimer -= Time.deltaTime;
        if (_rayTimer <= 0f)
        {
            _rayTimer = raycastInterval;
            DetectInteractableObject();
            HandleOutlineAndUI();
        }

        HandleInteract();
        // CheckForLadder();
    }

    void DetectInteractableObject()
    {
        var origin = playerCameraRoot ? playerCameraRoot.transform.position : transform.position;
        var dir = playerCameraRoot ? playerCameraRoot.transform.forward : transform.forward;

        if (Physics.Raycast(origin, dir, out var hit, GetComponent<Player>().reachRange, interactableMask))
            interactableObject = hit.collider.gameObject;

        else
            interactableObject = null;
    }

    void HandleOutlineAndUI()
    {
        if (lastOutlinedObject == interactableObject)
            return; // không đổi -> khỏi đụng UI/outline

        // Tắt cái cũ
        if (lastOutline) lastOutline.enabled = false;
        lastOutline = null;
        lastOutlinedObject = null;

        // Bật cái mới (nếu có)
        if (interactableObject)
        {
            // Gắn sẵn Outline trên prefab để tránh AddComponent lúc runtime.
            // Nếu buộc phải thêm tại runtime thì: try-get, nếu null thì Add ONCE rồi cache.
            var outline = interactableObject.GetComponent<Outline>();
            if (!outline) outline = interactableObject.AddComponent<Outline>(); // thêm 1 lần duy nhất
            outline.enabled = true;

            lastOutline = outline;
            lastOutlinedObject = interactableObject;
        }

        // UI chỉ update khi target đổi
        MultiKeyHintUI.interactTarget = interactableObject;
        if (keyHintUI) keyHintUI.UpdateAllKeyHints();
    }

    void HandleInteract()
    {
        if (interactAction == null || !interactAction.triggered || !interactableObject)
            return;
        var it = interactableObject.GetComponent<Interactable>();
        if (!it) return;

        // Dùng ref đã cache để tránh GetComponent mỗi lần
        switch (it.interactableType)
        {
            case Interactable.InteractableType.Takeable:
                Debug.Log("Take");
                playerAction.Take(interactableObject);
                break;
            case Interactable.InteractableType.Storage:
                Debug.Log("Store");
                playerAction.StoreItem(interactableObject);
                break;
            case Interactable.InteractableType.Climb:
                Debug.Log("Climb");
                playerAction.Climb(interactableObject);
                break;
            default:
                // Debug.Log($"Interactable type not handled: {it.interactableType}");
                break;
        }
    }
}
