using StarterAssets;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField]
    GameObject playerObject;
    [SerializeField]
    public GameObject onHoldingItem;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObject = this.gameObject;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
