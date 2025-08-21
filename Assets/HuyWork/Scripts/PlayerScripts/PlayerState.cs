using Mono.Cecil.Cil;
using StarterAssets;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField]
    GameObject playerObject;

    [SerializeField]
    public GameObject onHoldingItem;
    public MultiKeyHintUI keyHintUI;
    public GameObject playerCameraRoot;
    public Vector3 playerLookDirection;


    void Start()
    {
        playerObject = this.gameObject;
        if (!playerCameraRoot)
        {
            var fpc = GetComponent<FirstPersonController>();
            if (fpc != null) playerCameraRoot = fpc.CinemachineCameraTarget;
        }
        playerLookDirection = playerCameraRoot.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (onHoldingItem != null)
        {
            MultiKeyHintUI.isHoldingItem = true;
        }
        else
        {
            MultiKeyHintUI.isHoldingItem = false;
        }
    }
}
