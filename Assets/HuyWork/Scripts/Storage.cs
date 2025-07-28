using UnityEngine;
using System.Collections.Generic;

public class Storage : MonoBehaviour
{
    [SerializeField]
    List<GameObject> storageItems;
    int maxStorageCapacity = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void AddItem(GameObject item)
    {
        if (storageItems.Count < maxStorageCapacity && item != null && !storageItems.Contains(item))
        {
            storageItems.Add(item);
        }
    }
}
