using UnityEngine;
using UnityEngine.VFX;

public class WindToVFX : MonoBehaviour
{
    public WindZone windZone;
    public VisualEffect vfx;

    void Update()
    {
        if (windZone == null || vfx == null) return;

        // Hướng gió chính + độ mạnh
        Vector3 windDir = windZone.transform.forward * windZone.windMain;

        // Gửi vào VFX Graph
        vfx.SetVector3("WindDirection", windDir);
    }
}
