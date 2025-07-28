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
        if (playerState.onHoldingItem != null)
        {
            playerState.onHoldingItem.SetActive(false);
            playerState.onHoldingItem = null;
        }

        playerState.onHoldingItem = interactableObject;
        GetComponent<PlayerInventorySystem>().AddToInventory(interactableObject);
        Rigidbody rb = interactableObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        interactableObject.transform.SetParent(transform.Find("PlayerCameraRoot/Inventory"));
        interactableObject.transform.localPosition = new Vector3(0.3f, -0.2f, 1);
        interactableObject.transform.localScale = Vector3.one / 5;
    }

    public void StoreItem(GameObject interactableObject)
    {
        if (playerState.onHoldingItem == null) return;
        if (interactableObject == null) return;
        Debug.Log("Storing item: " + playerState.onHoldingItem.name + " into storage: " + interactableObject.name);
        Storage storage = interactableObject.GetComponent<Storage>();
        if (storage != null)
        {
            storage.AddItem(playerState.onHoldingItem);
            playerState.onHoldingItem.transform.SetParent(interactableObject.transform.Find("Inventory"));
            playerState.onHoldingItem.transform.localPosition = Vector3.zero;
            playerState.onHoldingItem.transform.localScale = Vector3.one;
            playerState.onHoldingItem = null;
        }
    }
}