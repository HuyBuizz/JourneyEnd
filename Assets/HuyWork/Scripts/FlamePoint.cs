using UnityEngine;

public class FlamePoint : MonoBehaviour
{
    [SerializeField]
    Transform pointContainer;
    [SerializeField]
    Transform model;
    [SerializeField]
    Transform effect;

    [SerializeField]
    float detectRadius = 0.0f;
    //////////////////////
    [SerializeField]
    float maxHealth = 0.0f;
    [SerializeField]
    float currentHealth = 0.0f;
    [SerializeField]
    float dps = 0.0f;

    void Update()
    {
        DetectSurroundingPoints();
        EffectHandler();
    }

    void Start()
    {
        pointContainer = transform.Find("PointContainer");
        if (pointContainer == null)
        {
            Debug.LogError("PointContainer not found.");
            return;
        }

        effect = transform.Find("Effect");
        if (effect == null)
        {
            Debug.LogError("Effect not found.");
            return;
        }

        model = transform.Find("Model");
        if (model == null)
        {
            Debug.LogError("Model not found.");
            return;
        }
    }

    void DetectSurroundingPoints()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("FlamePoint") && hitCollider.gameObject != this.gameObject)
            {
                FlamePoint flamePoint = hitCollider.GetComponent<FlamePoint>();
                if (flamePoint!= null)
                {
                    InflictFireDamage(flamePoint);
                }
            }
        }
    }

    void InflictFireDamage(FlamePoint FlamePoint)
    {
        if (this.currentHealth >= this.maxHealth / 2)
        {
            if (FlamePoint.currentHealth >= FlamePoint.maxHealth)
            {
                FlamePoint.currentHealth = FlamePoint.maxHealth;
            }
            else
            {
                FlamePoint.currentHealth += dps * Time.deltaTime;
            }
        }
    }

    void EffectHandler()
    {
        if (effect != null)
        {
            effect.gameObject.SetActive(currentHealth > maxHealth / 2);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}