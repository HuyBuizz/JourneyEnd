using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    private PlayerState playerState;

    void Start()
    {
        playerState = GetComponent<PlayerState>();
        if (playerState == null)
        {
            Debug.LogError("PlayerState component not found!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
        }
    }

    // public void Pickup(GameObject interactableObject)
    // {
    //     playerState.onHoldingObj = interactableObject;
    //     Rigidbody rb = interactableObject.GetComponent<Rigidbody>();
    //     if (rb != null)
    //     {
    //         rb.isKinematic = true;
    //     }
    //     interactableObject.transform.SetParent(transform.Find("PlayerCameraRoot"));
    //     interactableObject.transform.localPosition = new Vector3(0, 0, 1); // Adjust position to be above the player
    // }

    public void Drop()
    {
        if (playerState.onHoldingItem != null)
        {
            GameObject heldObject = playerState.onHoldingItem;
            playerState.onHoldingItem = null;

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            heldObject.transform.SetParent(null);
        }
    }

    // public void Throw()
    // {
    //     if (playerState.onHoldingObj != null)
    //     {
    //         GameObject heldObject = playerState.onHoldingObj;
    //         playerState.onHoldingObj = null;

    //         Rigidbody rb = heldObject.GetComponent<Rigidbody>();
    //         if (rb != null)
    //         {
    //             rb.isKinematic = false;
    //             rb.AddForce(transform.forward * 500);
    //         }
    //         heldObject.transform.SetParent(null);
    //     }
    // }

    public void Take(GameObject interactableObject)
    {
        if (interactableObject == null) return;

        // Nếu đang cầm vật phẩm khác thì ẩn nó đi và bỏ tham chiếu
        if (playerState.onHoldingItem != null)
        {
            playerState.onHoldingItem.SetActive(false);
            playerState.onHoldingItem = null;
        }

        // Cập nhật vật phẩm đang cầm
        playerState.onHoldingItem = interactableObject;

        // Thêm vào kho đồ
        var inventory = GetComponent<PlayerInventorySystem>();
        if (inventory != null)
        {
            inventory.AddToPlayerInventory(interactableObject);
        }

        // Thiết lập Rigidbody nếu có
        var rb = interactableObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Đặt lại vị trí, tỉ lệ, cha
        SetItemToInventorySlot(interactableObject);
        interactableObject.SetActive(true);
    }

    // Đặt vật phẩm vào kho đồ cá nhân
    private void SetItemToInventorySlot(GameObject item)
    {
        var inventoryRoot = transform.Find("PlayerCameraRoot/Inventory");
        if (inventoryRoot != null)
        {
            item.transform.SetParent(inventoryRoot);
            item.transform.localPosition = new Vector3(0.3f, -0.2f, 1);
            item.transform.localScale = Vector3.one / 5;
        }
    }
    // Cất item vào kho khác
    public void StoreItem(GameObject interactableObject)
    {
        if (playerState.onHoldingItem == null) return;
        if (interactableObject == null) return;
        Storage storage = interactableObject.GetComponent<Storage>();
        if (storage != null)
        {
            storage.TransferItemToStorage(playerState.onHoldingItem, this.gameObject);
            playerState.onHoldingItem = null;
        }
    }
}