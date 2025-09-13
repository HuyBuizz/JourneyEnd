using StarterAssets;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3 playerLookDirection;

    [SerializeField]
    GameObject playerCameraRoot;

    [SerializeField]
    GameObject crosshairObject;

    /// <summary>
    /// Player stats
    /// </summary>
    public float health = 100f;
    public float stamina = 100f;
    public float reachRange = 5f;
    public float climbSpeed = 3.0f;
    [Header("PlayerTempData")]
    public float ClimableHeight;

    void Start()
    {
        playerCameraRoot = this
            .gameObject.GetComponent<FirstPersonController>()
            .CinemachineCameraTarget;
    }

    // Update is called once per frame
    void Update()
    {
        PlaceCrosshair();
        playerLookDirection = playerCameraRoot.transform.forward;
    }

    void PlaceCrosshair()
    {
        if (crosshairObject != null && playerCameraRoot != null)
        {
            RaycastHit hit;
            if (
                Physics.Raycast(
                    playerCameraRoot.transform.position,
                    playerCameraRoot.transform.forward,
                    out hit,
                    reachRange,
                    LayerMask.GetMask("Interactable")
                )
            )
            {
                crosshairObject.transform.position = hit.point;
            }
            else
            {
                crosshairObject.transform.position =
                    playerCameraRoot.transform.position
                    + playerCameraRoot.transform.forward * reachRange;
            }
        }
    }
}
