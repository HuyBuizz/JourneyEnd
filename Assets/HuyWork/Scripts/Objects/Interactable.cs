using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractableType
    {
        Takeable,
        Storage,

    }
    public InteractableType interactableType;
}
