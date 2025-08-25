using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractableType
    {
        Takeable,
        Storage,
        Climb

    }
    public InteractableType interactableType;
}
