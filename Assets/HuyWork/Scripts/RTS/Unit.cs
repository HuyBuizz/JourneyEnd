using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool isSelected = false;



    /// <summary>
    /// ////////// Trạng thái hiện tại của Unit.
    /// </summary>
    public enum UnitState { Idle, Moving, Pickup }
    public UnitState currentState = UnitState.Idle;
}
