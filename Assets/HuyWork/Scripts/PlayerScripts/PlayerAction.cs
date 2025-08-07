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
    /// </summary>
    public void Take(GameObject interactableObject)
    {
        if (interactableObject == null) return;

        // Nếu đang cầm vật phẩm khác thì ẩn nó đi và bỏ khỏi inventory
        if (playerState.onHoldingItem != null)
        {
            RemoveCurrentItem();
        }

        playerState.onHoldingItem = interactableObject;
        AddItemToInventory(interactableObject);
        SetupItemForInventory(interactableObject);
    }

    /// <summary>
    /// Buông vật phẩm đang cầm
    /// </summary>
    public void Drop()
    {
        if (playerState.onHoldingItem == null) return;

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
        if (playerState.onHoldingItem == null || interactableObject == null) return;

        Storage storage = interactableObject.GetComponent<Storage>();
        if (storage == null) return;

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
            currentItem.SetActive(false);
            currentItem.transform.SetParent(null);
        }
        playerState.onHoldingItem = null;
    }
}