// KeyActionMapping.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "KeyActionMapping", menuName = "Input/KeyActionMapping")]
public class KeyActionMapping : ScriptableObject
{
    public List<KeyAction> KeyActions;
}

[Serializable]
public struct KeyAction
{
    public Key KeyCode;
    public string Action;
    public ShowCondition Condition; // << thêm
}

public enum ConditionMode
{
    Always,         // luôn hiển thị (ví dụ WASD)
    RequireFlag,    // chỉ hiển thị khi 1 flag đang bật
    RequireNotFlag  // chỉ hiển thị khi 1 flag đang tắt
}

[Serializable]
public struct ShowCondition
{
    public ConditionMode Mode;
    public string Flag; // tên flag ngữ cảnh (ví dụ "NearLadder", "HasInteractTarget")
}
