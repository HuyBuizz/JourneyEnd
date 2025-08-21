using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    public GameObject equipper;
    public enum ItemType
    {
        FireExtinguisher,
        FireAxe,
        FireHose,
    }

    public ItemType itemType;
}
