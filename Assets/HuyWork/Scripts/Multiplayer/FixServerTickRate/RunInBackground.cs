// Assets/Scripts/Boot/RunInBackground.cs
using UnityEngine;

public class RunInBackground
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ForceRunInBackground()
    {
        Application.runInBackground = true;   // bắt buộc
        QualitySettings.vSyncCount = 0;       // tránh bị khóa theo refresh màn hình
        Application.targetFrameRate = 120;    // đảm bảo fps > tickrate
    }
}
