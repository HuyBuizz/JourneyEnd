// 8/22/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using StarterAssets;
using Unity.InferenceEngine;

public class LadderClimbing : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 3.0f;
    public LayerMask ladderLayer;
    public Transform playerTransform;

    private CharacterController characterController;
    private StarterAssetsInputs inputs;
    private FirstPersonController firstPersonController;
    private GameObject playerCameraRoot;
    private PlayerState playerState;


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        inputs = GetComponent<StarterAssetsInputs>();
        firstPersonController = GetComponent<FirstPersonController>();
        playerCameraRoot = firstPersonController.CinemachineCameraTarget;
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        CheckForLadder();

        if (Input.GetKey(KeyCode.F) && playerState.isInClimbableState && !playerState.isPLayerClimbing)
        {
            playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y + 0.2f, playerTransform.position.z);
            playerState.isPLayerClimbing = true;
        }

        if (playerState.isInClimbableState && playerState.isPLayerClimbing)
        {
            ClimbLadder();
        }
        else
        {
            StopClimbing();
        }
    }

    private void CheckForLadder()
    {
        // Cast a ray to detect ladders
        RaycastHit hit;
        if (Physics.Raycast(playerCameraRoot.transform.position, playerCameraRoot.transform.forward, out hit, 1.0f, ladderLayer))
        {
            playerState.isInClimbableState = true;
        }
        else
        {
            playerState.isInClimbableState = false;
        }
    }

    private void StopClimbing()
    {
        playerState.isPLayerClimbing = false;
        characterController.enabled = true; // Re-enable CharacterController
    }

    private void ClimbLadder()
    {
        Vector3 climbDirection = new Vector3(0, inputs.move.y * climbSpeed, 0);
        transform.Translate(climbDirection * Time.deltaTime);

        // Stop climbing if grounded
        if (firstPersonController.Grounded)
        {
            StopClimbing();
        }
    }
}