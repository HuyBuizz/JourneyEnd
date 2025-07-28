using UnityEngine;

public class PlayerInteractionSystem : MonoBehaviour
{
    [SerializeField]
    GameObject playerCameraRoot;
    [SerializeField]
    GameObject interactableObject;

    GameObject lastOutlinedObject;

    void Start()
    {
        playerCameraRoot = GameObject.Find("PlayerCameraRoot");
        if (playerCameraRoot == null)
        {
            Debug.LogError("PlayerCameraRoot not found in the scene. Please ensure it exists.");
        }
        else
        {
            Debug.Log("PlayerCameraRoot found successfully.");
        }
    }

    void Update()
    {
        DetectInteractableObject();
        OutLineHandler();
        Interact();
    }

    void DetectInteractableObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCameraRoot.transform.position, playerCameraRoot.transform.forward, out hit, 5f, LayerMask.GetMask("Interactable")))
        {
            interactableObject = hit.collider.gameObject;
        }
        else
        {
            interactableObject = null;
        }
    }

    void OutLineHandler()
    {
        if (lastOutlinedObject != null && lastOutlinedObject != interactableObject)
        {
            Outline outline = lastOutlinedObject.GetComponent<Outline>();
            if (outline != null)
            {
                Destroy(outline);
            }
            lastOutlinedObject = null;
        }

        if (interactableObject != null)
        {
            Outline outline = interactableObject.GetComponent<Outline>();
            if (outline == null)
            {
                interactableObject.AddComponent<Outline>();
            }
            lastOutlinedObject = interactableObject;
        }
    }

    void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E) && interactableObject != null)
        {
            if (interactableObject.GetComponent<Interactable>() == null)
            {
                return;
            }
            switch (interactableObject.GetComponent<Interactable>().interactableType)
            {
                case Interactable.InteractableType.Takeable:
                    this.GetComponent<PlayerAction>().Take(interactableObject);
                    break;
                case Interactable.InteractableType.Storage:
                    // this.GetComponent<PlayerAction>().OpenStorage(interactableObject);
                    this.GetComponent<PlayerAction>().StoreItem(interactableObject);
                    break;
                default:
                    Debug.Log("Interactable type not handled: " + interactableObject.GetComponent<Interactable>().interactableType);
                    break;
            }
        }
    }

    
}