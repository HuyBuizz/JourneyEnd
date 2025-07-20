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

        if (Input.GetKeyDown(KeyCode.F))
        {
            Throw();
        }
    }

    public void Pickup(GameObject interactableObject)
    {
        playerState.onHoldingObj = interactableObject;
        Rigidbody rb = interactableObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        interactableObject.transform.SetParent(transform.Find("PlayerCameraRoot"));
        interactableObject.transform.localPosition = new Vector3(0, 0, 1); // Adjust position to be above the player
    }

    public void Drop()
    {
        if (playerState.onHoldingObj != null)
        {
            GameObject heldObject = playerState.onHoldingObj;
            playerState.onHoldingObj = null;

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            heldObject.transform.SetParent(null);
        }
    }

    public void Throw()
    {
        if (playerState.onHoldingObj != null)
        {
            GameObject heldObject = playerState.onHoldingObj;
            playerState.onHoldingObj = null;

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * 500);
            }
            heldObject.transform.SetParent(null);
        }
    }

    public void Take(GameObject interactableObject)
    {
        if (playerState.onHoldingObj != null) return;

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


}