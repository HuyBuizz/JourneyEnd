using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    public CharacterController controller;
    public Vector3 velocity;
    public bool isGrounded;
    public Vector2 moveInput;

    public void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Update()
    {
        // Kiểm tra chạm đất
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Di chuyển
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.TransformDirection(move);
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Trọng lực
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // PlayerInput -> Send Messages
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            Debug.Log("Jumping");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}
