using UnityEngine;

public class LightRotator : MonoBehaviour
{
    // Tốc độ xoay (độ/giây), bạn có thể chỉnh trong Inspector
    public float rotationSpeed = 50f;

    void Update()
    {
        // Dòng này là quan trọng nhất!
        // Vector3.up sẽ xoay đối tượng quanh trục Y (trục thẳng đứng)
        // -> tạo ra chuyển động xoay ngang.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}