using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private GameObject equipper;
    public enum ItemType
    {
        Equipment,

    }

    public ItemType itemType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SetEquipper();
    }

    void SetEquipper()
    {
        if (transform.parent == null ||
    transform.parent.parent == null ||
    transform.parent.parent.parent == null) return;
        if (transform.parent.parent.parent.gameObject.GetComponent<Player>() != null)
        {
            equipper = transform.parent.parent.parent.gameObject;
        }
        else
        {
            equipper = null;
        }
    }
}
