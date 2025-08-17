using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    public Transform cabin; // Cabin của thang máy
    public Transform[] waypoints; // Các tầng (Point1, Point2, ...)
    public float speed = 2f; // Tốc độ di chuyển

    private int currentFloor = 0; // Tầng hiện tại
    private bool isMoving = false;

    void Update()
    {
        if (isMoving)
        {
            MoveElevator();
        }
    }

    public void GoToFloor(int floorIndex)
    {
        if (floorIndex != currentFloor && !isMoving)
        {
            currentFloor = floorIndex;
            isMoving = true;
        }
    }

    private void MoveElevator()
    {
        Transform target = waypoints[currentFloor];
        cabin.position = Vector3.MoveTowards(
            cabin.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(cabin.position, target.position) < 0.01f)
        {
            isMoving = false;
        }
    }
}
