using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    // Input
    private float horizontalInput, verticalInput;
    private bool isBraking;

    // Steering & motor
    private float currentSteerAngle, currentBrakeForce;

    [SerializeField] private GameObject driver;

    [Header("Car Settings")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private Vector3 wheelsOffset = Vector3.zero;

    [Header("Invert Controls")]
    [Tooltip("Tick để đảo ngược W/S")]
    public bool invertControls = false; // ✅ Khi tick sẽ đảo ngược W/S

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("Wheel Transforms")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        ApplyStabilizerBar(frontLeftWheelCollider, frontRightWheelCollider);
        ApplyStabilizerBar(rearLeftWheelCollider, rearRightWheelCollider);
    }

    private void GetInput()
    {
        if (driver == null) return;
        horizontalInput = Input.GetAxis("Horizontal"); // A/D hoặc trái/phải
        verticalInput = Input.GetAxis("Vertical");     // W/S hoặc lên/xuống

        // Nếu tick invertControls thì đảo ngược giá trị vertical
        if (invertControls)
        {
            verticalInput *= -1f;
        }

        isBraking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        float speed = rb.linearVelocity.magnitude;
        float adjustedBrakeForce = Mathf.Lerp(brakeForce, brakeForce / 2f, speed / 50f);
        currentBrakeForce = isBraking ? adjustedBrakeForce : 0f;

        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;

        // Motor bánh trước
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
    }

    private void HandleSteering()
    {
        float speed = rb.linearVelocity.magnitude;
        float adjustedSteer = Mathf.Lerp(maxSteerAngle, maxSteerAngle / 2f, speed / 50f);
        currentSteerAngle = adjustedSteer * horizontalInput;

        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot * Quaternion.Euler(wheelsOffset);
    }

    private void ApplyStabilizerBar(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit hit;
        float travelLeft = 1.0f;
        float travelRight = 1.0f;

        if (leftWheel.GetGroundHit(out hit))
            travelLeft = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        if (rightWheel.GetGroundHit(out hit))
            travelRight = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;

        float antiRollForce = (travelLeft - travelRight) * 5000f;

        if (leftWheel.isGrounded)
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        if (rightWheel.isGrounded)
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
    }
}
