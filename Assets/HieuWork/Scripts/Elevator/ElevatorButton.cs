using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    public ElevatorController elevator;
    public int targetFloor;

    public void OnButtonPress()
    {
        elevator.GoToFloor(targetFloor);
    }
}
