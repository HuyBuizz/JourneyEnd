using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    private PlayerState playerState;
    private Player player;
    private PlayerInventorySystem inventory;

    [SerializeField]
    PlayerInput playerInput;

    InputAction interactAction;
    [SerializeField]
    public MultiKeyHintUI keyHintUI;
    public Vector3 itemGUIPosition = new Vector3(0.3f, -0.2f, 1);

    void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
        
        if (playerState == null)
            playerState = GetComponent<PlayerState>();

        if (player == null)
            player = GetComponent<Player>();

        if (inventory == null)
            inventory = GetComponent<PlayerInventorySystem>();


        // Lấy action "Drop" từ PlayerInput
        interactAction = playerInput.actions["Drop"];
    }

    void OnEnable()
    {
        if (interactAction != null)
            interactAction.Enable();
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.Disable();
    }

    void Update()
    {
        if (interactAction.triggered)
        {
            Drop();
            // Cập nhật key hints sau khi drop
        }

        SetItemGUIPosition();
    }

    public void Take(GameObject interactableObject)
    {
        if (interactableObject == null)
            return;

        // Nếu đang cầm vật phẩm khác
        if (playerState.onHoldingItem != null)
        {
            // Nếu inventory còn chỗ thì chỉ ẩn vật phẩm đang cầm đi (không drop)
            if (inventory != null && inventory.ItemCount < inventory.MaxInventorySize)
            {
                HideCurrentItemToInventory();
            }
            else
            {
                // Nếu inventory đầy thì drop vật phẩm đang cầm
                RemoveCurrentItem();
            }
        }

        // Chỉ thêm và setup nếu inventory còn chỗ
        if (inventory != null && inventory.AddToPlayerInventory(interactableObject))
        {
            playerState.onHoldingItem = interactableObject;
            SetupItemForInventory(interactableObject);
        }
        else
        {
            Debug.Log("Không thể nhặt thêm vật phẩm, túi đã đầy!");
            // Có thể thêm hiệu ứng/thông báo UI ở đây nếu muốn
        }
    }

    // Thêm hàm này để ẩn vật phẩm đang cầm và giữ trong inventory
    private void HideCurrentItemToInventory()
    {
        GameObject currentItem = playerState.onHoldingItem;
        if (currentItem != null)
        {
            currentItem.SetActive(false);
            currentItem.transform.SetParent(transform.Find("PlayerCameraRoot/Inventory"));
            // Không cần RemoveItemFromInventory vì vẫn giữ trong inventory
        }
        playerState.onHoldingItem = null;
    }

    /// <summary>
    /// Buông vật phẩm đang cầm
    /// </summary>
    public void Drop()
    {
        if (playerState.onHoldingItem == null)
            return;

        GameObject heldObject = playerState.onHoldingItem;
        playerState.onHoldingItem = null;

        RemoveItemFromInventory(heldObject);
        SetupItemForWorld(heldObject);

        // >>> CẬP NHẬT TRẠNG THÁI VÀ HINT
        MultiKeyHintUI.isHoldingItem = false;
        if (keyHintUI != null) keyHintUI.UpdateAllKeyHints();
    }

    /// <summary>
    /// Cất vật phẩm vào kho khác
    /// </summary>
    public void StoreItem(GameObject interactableObject)
    {
        if (playerState.onHoldingItem == null || interactableObject == null)
            return;

        Storage storage = interactableObject.GetComponent<Storage>();
        if (storage == null)
            return;

        RemoveItemFromInventory(playerState.onHoldingItem);
        storage.TransferItemToStorage(playerState.onHoldingItem, gameObject);
        playerState.onHoldingItem = null;
    }

    /// <summary>
    /// Đặt vật phẩm vào kho cá nhân
    /// </summary>
    private void SetupItemForInventory(GameObject item)
    {
        var inventoryRoot = transform.Find("PlayerCameraRoot/Inventory");
        if (inventoryRoot != null)
        {
            item.transform.SetParent(inventoryRoot);
            item.transform.localRotation = Quaternion.identity;

            // item.transform.localScale = Vector3.one / 5;
        }

        var rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        item.SetActive(true);
        item.GetComponent<Item>().equipper = gameObject;
    }

    private void SetItemGUIPosition()
    {
        var inventoryRoot = transform.Find("PlayerCameraRoot/Inventory");
        foreach (Transform item in inventoryRoot.transform)
        {
            item.transform.localPosition = itemGUIPosition;
        }
    }

    /// <summary>
    /// Đặt vật phẩm ra ngoài thế giới khi drop
    /// </summary>
    private void SetupItemForWorld(GameObject item)
    {
        item.transform.SetParent(null);

        var rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        item.SetActive(true);
        item.GetComponent<Item>().equipper = null;
    }

    /// <summary>
    /// Thêm vật phẩm vào inventory
    /// </summary>
    private void AddItemToInventory(GameObject item)
    {
        if (inventory != null && item != null)
        {
            inventory.AddToPlayerInventory(item);
        }
    }

    /// <summary>
    /// Loại vật phẩm khỏi inventory
    /// </summary>
    private void RemoveItemFromInventory(GameObject item)
    {
        if (inventory != null && item != null)
        {
            inventory.RemoveFromPlayerInventory(item);
        }
    }

    /// <summary>
    /// Xử lý khi đang cầm vật phẩm khác
    /// </summary>
    private void RemoveCurrentItem()
    {
        GameObject currentItem = playerState.onHoldingItem;
        if (currentItem != null)
        {
            RemoveItemFromInventory(currentItem);
            // currentItem.SetActive(true);
            currentItem.transform.SetParent(null);
            SetupItemForWorld(currentItem);
        }
        playerState.onHoldingItem = null;
    }

    public void Climb(GameObject ClimableObject)
    {
        // Adjust player position slightly upwards to avoid Grounded state
        if (!playerState.isPlayerClimbing)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);
            playerState.isPlayerClimbing = true;
            player.ClimableHeight = ClimableObject.GetComponent<Collider>().bounds.max.y;
        }
    }
}
