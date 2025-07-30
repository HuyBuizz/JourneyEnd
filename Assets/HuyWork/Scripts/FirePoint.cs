using UnityEngine;

public class FirePoint : MonoBehaviour
{
    [SerializeField]
    Transform pointContainer;
    [SerializeField]
    Transform model;
    [SerializeField]
    Transform effect;
    [SerializeField]
    float distanceToActive = 0.0f;
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
        FatherPointActiveWhenPlayerInRange();
        ChildPointActiveWhenPlayerInRange();
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

    void ChildPointActiveWhenPlayerInRange()
    {
        bool playerFound = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distanceToActive);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                playerFound = true;
                break;
            }
        }

        if (pointContainer == null)
        {
            Debug.LogError("PointContainer is null in FirePoint.");
            return;
        }

        foreach (Transform point in pointContainer)
        {
            foreach (Transform model in point.Find("Model"))
            {
                model.gameObject.SetActive(playerFound);
            }
        }
    }

    void FatherPointActiveWhenPlayerInRange()
    {
        bool playerFound = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distanceToActive);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                playerFound = true;
                break;
            }
        }

        if (model == null)
        {
            Debug.LogError("Model is null in FirePoint.");
            return;
        }

        model.gameObject.SetActive(playerFound);
        if (effect != null)
        {
            effect.gameObject.SetActive(playerFound);
        }
        else
        {
            Debug.LogWarning("Effect is null in FirePoint.");
        }
    }

    void DetectSurroundingPoints()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("FirePoint") && hitCollider.gameObject != this.gameObject)
            {
                FirePoint firePoint = hitCollider.GetComponent<FirePoint>();
                if (firePoint != null)
                {
                    InflictFireDamage(firePoint);
                }
            }
        }
    }

    void InflictFireDamage(FirePoint firePoint)
    {
        if (this.currentHealth >= this.maxHealth / 2)
        {
            if (firePoint.currentHealth >= firePoint.maxHealth)
            {
                firePoint.currentHealth = firePoint.maxHealth;
            }
            else
            {
                firePoint.currentHealth += dps * Time.deltaTime;
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