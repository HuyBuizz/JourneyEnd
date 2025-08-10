using StarterAssets;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    // [SerializeField] Vector3 playerLookDirection;
    [SerializeField] GameObject playerCameraRoot;
    [SerializeField] GameObject crosshairObject;
    public float health = 100f;
    public float stamina = 100f;
    public float reachRange = 5f;

    public EntityManager entityManager;
    public Entity playerPosEntity;

    void Start()
    {
        playerCameraRoot = this.gameObject.GetComponent<FirstPersonController>().CinemachineCameraTarget;
        // playerLookDirection = playerCameraRoot.transform.forward;

        // Khởi tạo entity singleton cho PlayerPosition
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(PlayerPosition));
        if (query.IsEmpty)
            playerPosEntity = entityManager.CreateEntity(typeof(PlayerPosition));
        else
            playerPosEntity = query.GetSingletonEntity();
    }

    void Update()
    {
        PlaceCrosshair();

        // Sync vị trí player vào DOTS mỗi frame
        if (entityManager.Exists(playerPosEntity))
        {
            entityManager.SetComponentData(playerPosEntity, new PlayerPosition { Value = transform.position });
        }
    }

    void PlaceCrosshair()
    {
        if (crosshairObject != null && playerCameraRoot != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCameraRoot.transform.position, playerCameraRoot.transform.forward, out hit, reachRange, LayerMask.GetMask("Interactable")))
            {
                crosshairObject.transform.position = hit.point;
            }
            else
            {
                crosshairObject.transform.position = playerCameraRoot.transform.position + playerCameraRoot.transform.forward * reachRange;
            }
        }
    }
}

public struct PlayerPosition : IComponentData
{
    public float3 Value;
}