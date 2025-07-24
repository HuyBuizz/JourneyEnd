using UnityEngine;

public class FirePoint : MonoBehaviour
{
    [SerializeField]
    float distanceToActive = 1.0f;

    void Update()
    {
        PointActiveWhenPlayerInRange();
    }

    void PointActiveWhenPlayerInRange()
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

        foreach (Transform point in transform.Find("PointContainer"))
        {
            point.gameObject.SetActive(playerFound);
        }
    }
}