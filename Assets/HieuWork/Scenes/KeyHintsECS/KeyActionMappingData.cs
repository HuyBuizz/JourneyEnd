using Unity.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;

public struct KeyActionMappingData : IBufferElementData
{
    public Key KeyCode;
    public FixedString64Bytes Action;
}
