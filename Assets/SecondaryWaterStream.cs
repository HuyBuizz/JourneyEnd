using UnityEngine;

public class SecondaryWaterStream : MonoBehaviour
{
    public float startSpeed = 15f;
    public float spreadAngle = 5f;
    public float gravityModifier = 2f;
    public float collisionDampen = 0.3f;
    public float lifetime = 2f;

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ApplySettings();
    }

    void Update()
    {
        // Continuously apply settings so inspector changes update live
        ApplySettings();
    }

    void ApplySettings()
    {
        var main = ps.main;
        main.startSpeed = startSpeed;
        main.startLifetime = lifetime;
        main.gravityModifier = gravityModifier;

        var shape = ps.shape;
        shape.angle = spreadAngle;

        var collision = ps.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.dampen = collisionDampen;
        collision.bounce = 0.05f;
        collision.lifetimeLoss = 0.7f;
    }
}   