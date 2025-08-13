using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    private PlayerState playerState;
    private PlayerInventorySystem inventory;

    void Start()
    {
        playerState = GetComponent<PlayerState>();
        inventory = GetComponent<PlayerInventorySystem>();

        if (playerState == null)
            Debug.LogError("PlayerState component not found!");
        if (inventory == null)
            Debug.LogError("PlayerInventorySystem component not found!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
        }
    }

    /// <summary>
    /// Nhặt một vật phẩm và thêm vào kho cá nhân
    // /// </summary>

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
            item.transform.localPosition = new Vector3(0.3f, -0.2f, 1);
            item.transform.localScale = Vector3.one / 5;
        }

        var rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        item.SetActive(true);
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
}
