using StarterAssets;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3 playerLookDirection;

    [SerializeField]
    GameObject playerCameraRoot;

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
        playerLookDirection = playerCameraRoot.transform.forward;
    }
}
