using Unity.Entities;

[System.Flags]
public enum PhysicsCategory : uint
{
    Character = 1 << 3,
    Interactable = 1 << 6,
    Ground = 1 << 9,
}