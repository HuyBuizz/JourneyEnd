using UnityEngine;

public class FireHoseDropEquipV2 : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("Empty transform on the hand/rig where the hose should attach when equipped.")]
    public Transform hoseHolder;

    [Tooltip("Root GameObject of the hose in the scene (has RB/Colliders).")]
    public GameObject fireHose;

    [Tooltip("Player transform used to measure distance (e.g., the player root).")]
    public Transform player;

    [Header("Spray (optional)")]
    [Tooltip("All particle systems that make up the water spray (main + secondary).")]
    public ParticleSystem[] spraySystems;

    [Header("Input")]
    public KeyCode equipKey = KeyCode.E;     // Equip / Unequip
    public KeyCode sprayKey = KeyCode.Mouse0; // Toggle spray on / off

    [Header("Rules")]
    [Tooltip("Max distance from the hose required to equip.")]
    public float equipRange = 3f;

    [Tooltip("If true, will try to auto-find Player by tag if 'player' is not set.")]
    public bool autoFindPlayerByTag = true;
    public string playerTag = "Player";

    // Internals
    private Rigidbody hoseRb;
    private bool isEquipped = false;
    private bool isSpraying = false;

    // For safe reattach (scale issues)
    private Vector3 hoseInitialLocalScale;

    void Awake()
    {
        if (fireHose == null)
        {
            Debug.LogError("[FireHoseDropEquipV2] 'fireHose' is not assigned.");
            enabled = false;
            return;
        }

        // Cache RB (works if on child too)
        hoseRb = fireHose.GetComponentInChildren<Rigidbody>(true);
        if (hoseRb == null)
        {
            Debug.LogWarning("[FireHoseDropEquipV2] No Rigidbody found on hose. Adding one for drop physics.");
            hoseRb = fireHose.AddComponent<Rigidbody>();
        }

        // Store initial local scale so we can reapply when re-parenting
        hoseInitialLocalScale = fireHose.transform.localScale;

        // Make sure sprays are stopped on boot
        SetSprayEnabled(false, clear: true);
    }

    void Start()
    {
        // Try to find player if not assigned
        if (player == null && autoFindPlayerByTag)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        if (hoseHolder == null)
        {
            Debug.LogError("[FireHoseDropEquipV2] 'hoseHolder' is not assigned.");
            enabled = false;
            return;
        }

        // Start UNEQUIPPED -> drop out of the model
        Unequip(dropOnGround: true);
    }

    void Update()
    {
        if (fireHose == null || hoseHolder == null) return;

        if (Input.GetKeyDown(equipKey))
        {
            if (isEquipped)
            {
                Unequip(dropOnGround: true);
            }
            else
            {
                // Range check
                if (player == null)
                {
                    Debug.LogWarning("[FireHoseDropEquipV2] No player Transform assigned; equipping without range check.");
                    Equip();
                }
                else
                {
                    float dist = Vector3.Distance(player.position, fireHose.transform.position);
                    if (dist <= equipRange) Equip();
                    else Debug.Log("Too far to equip hose.");
                }
            }
        }

        if (isEquipped && Input.GetKeyDown(sprayKey))
        {
            isSpraying = !isSpraying;
            SetSprayPlaying(isSpraying);
        }
    }

    // ---- Core actions ----

    private void Equip()
    {
        isEquipped = true;
        isSpraying = false;

        // Attach to holder (avoid scale warping)
        fireHose.transform.SetParent(hoseHolder, worldPositionStays: false);
        fireHose.transform.localPosition = Vector3.zero;
        fireHose.transform.localRotation = Quaternion.identity;
        fireHose.transform.localScale = hoseInitialLocalScale;

        // Disable physics while equipped
        if (hoseRb != null)
        {
            hoseRb.isKinematic = true;
            hoseRb.detectCollisions = false;
            hoseRb.linearVelocity = Vector3.zero;
            hoseRb.angularVelocity = Vector3.zero;
        }

        // Make spray available but not yet playing
        SetSprayEnabled(true, clear: false);
        SetSprayPlaying(false);
    }

    private void Unequip(bool dropOnGround)
    {
        isEquipped = false;
        isSpraying = false;

        // Stop spray + disable emission so it truly stops
        SetSprayPlaying(false);
        SetSprayEnabled(false, clear: true);

        // Detach and enable physics so it drops
        fireHose.transform.SetParent(null, worldPositionStays: true);

        if (hoseRb != null)
        {
            hoseRb.isKinematic = false;
            hoseRb.detectCollisions = true;

            if (dropOnGround)
            {
                // Small downward nudge so it cleanly falls out of the hand
                hoseRb.AddForce(Vector3.down * 0.25f, ForceMode.VelocityChange);
            }
        }
    }

    // ---- Spray helpers ----

    private void SetSprayEnabled(bool enabled, bool clear)
    {
        if (spraySystems == null) return;

        foreach (var ps in spraySystems)
        {
            if (ps == null) continue;
            var emission = ps.emission;
            emission.enabled = enabled;

            if (!enabled)
            {
                if (clear) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                else ps.Stop();
            }
        }
    }

    private void SetSprayPlaying(bool play)
    {
        if (spraySystems == null) return;

        foreach (var ps in spraySystems)
        {
            if (ps == null) continue;
            if (play)
            {
                // Ensure emission is enabled before Play
                var emission = ps.emission;
                emission.enabled = true;
                if (!ps.isPlaying) ps.Play();
            }
            else
            {
                if (ps.isPlaying) ps.Stop();
            }
        }
    }

#if UNITY_EDITOR
    // Visualize equip range in Scene view
    void OnDrawGizmosSelected()
    {
        if (player == null || fireHose == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, equipRange);
    }
#endif
}
