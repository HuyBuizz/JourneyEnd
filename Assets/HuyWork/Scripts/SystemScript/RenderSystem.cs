using System.Collections.Generic;
using UnityEngine;

public class RenderSystem : MonoBehaviour
{
    [SerializeField]
    float renderDistance = 0.0f;

    private HashSet<GameObject> renderedObjs = new HashSet<GameObject>();
    void Start()
    {
        InvokeRepeating("RenderObjsInRange", 0.5f, 0.5f);
    }

    void Update()
    {
        // RenderObjsInRange();
    }

    void RenderObjsInRange()
    {
        // Lưu lại các obj hiện tại trong tầm
        HashSet<GameObject> objsInRange = new HashSet<GameObject>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, renderDistance);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("FlamePoint"))
            {
                Transform model = hitCollider.transform.Find("Model");
                if (model != null)
                {
                    foreach (Transform item in model)
                    {
                        item.gameObject.SetActive(true);
                        objsInRange.Add(item.gameObject);
                    }
                }
            }
        }

        // Ẩn các obj đã render nhưng hiện không còn trong tầm
        foreach (var obj in renderedObjs)
        {
            if (!objsInRange.Contains(obj))
            {
                obj.SetActive(false);
            }
        }

        // Cập nhật danh sách đã render
        renderedObjs = objsInRange;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, renderDistance);
    }
}