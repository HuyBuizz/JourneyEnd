using Mono.Cecil.Cil;
using StarterAssets;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField]
    GameObject playerObject;
    public MultiKeyHintUI keyHintUI;
    private GameObject playerCameraRoot;
    public Vector3 playerLookDirection;
    [Header("Player State")]
    public GameObject onHoldingItem;

    public bool isInClimbableState = false;
    public bool isPlayerClimbing = false;


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
        ResetTempData();
    }

    public void ResetTempData()
    {
        if (isPlayerClimbing == false)
        {
            GetComponent<Player>().ClimableHeight = 0f;
        }
    }
}
