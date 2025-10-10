using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject owner;
    public enum InteractableType
    {
        Takeable,
        Storage,
        Climb

    }
    public InteractableType interactableType;
}
