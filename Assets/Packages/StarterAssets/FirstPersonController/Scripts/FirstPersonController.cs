using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;

        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip(
            "Time required to pass before being able to jump again. Set to 0f to instantly jump again"
        )]
        public float JumpTimeout = 0.1f;

        [Tooltip(
            "Time required to pass before entering the fall state. Useful for walking down stairs"
        )]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip(
            "If the character is grounded or not. Not part of the CharacterController built in grounded check"
        )]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip(
            "The radius of the grounded check. Should match the radius of the CharacterController"
        )]
        public float GroundedRadius = 0.5f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip(
            "The follow target set in the Cinemachine Virtual Camera that the camera will follow"
        )]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError(
                "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it"
            );
#endif

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );
            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (Grounded)
            {
                GetComponent<PlayerState>().isPlayerClimbing = false;
            }
        }

        private void CameraRotation()
        {
            // if there is an input
            if (_input.look.sqrMagnitude >= _threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(
                    _cinemachineTargetPitch,
                    BottomClamp,
                    TopClamp
                );

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(
                    _cinemachineTargetPitch,
                    0.0f,
                    0.0f
                );

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            var playerState = GetComponent<PlayerState>();

            // Nếu đang leo, chỉ cho phép đi lên hoặc xuống
            if (playerState.isPlayerClimbing)
            {
                if (transform.position.y >= GetComponent<Player>().ClimableHeight + 1f)
                {
                    playerState.isPlayerClimbing = false;
                    return;
                }

                float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

                // Nếu không có input lên/xuống thì tốc độ bằng 0
                float verticalInput = _input.move.y; // sử dụng trục y của Vector2 input
                if (verticalInput == 0f)
                    targetSpeed = 0f;

                // Lerp tốc độ để mềm mại
                _speed = Mathf.Lerp(_speed, targetSpeed * Mathf.Abs(verticalInput), Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;

                // Tạo hướng chỉ theo trục y
                Vector3 climbDirection = Vector3.up * verticalInput;

                // Di chuyển nhân vật
                _controller.Move(climbDirection.normalized * (_speed * Time.deltaTime));

                // Không đi ngang khi leo
                return;
            }

            // --- Phần di chuyển bình thường (không leo) ---
            float normalTargetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero)
                normalTargetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(
                _controller.velocity.x,
                0.0f,
                _controller.velocity.z
            ).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < normalTargetSpeed - speedOffset
                || currentHorizontalSpeed > normalTargetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    normalTargetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = normalTargetSpeed;
            }

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            _controller.Move(
                inputDirection.normalized * (_speed * Time.deltaTime)
                + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );
        }

        private void JumpAndGravity()
        {
            var playerState = GetComponent<PlayerState>();

            // Nếu đang grounded
            if (Grounded)
            {
                // reset fall timeout
                _fallTimeoutDelta = FallTimeout;

                // ngăn velocity âm khi đứng trên mặt đất
                if (_verticalVelocity < 0.0f)
                    _verticalVelocity = -2f;

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    playerState.isPlayerClimbing = false; // nếu đang leo, thoát leo khi nhảy
                }

                // giảm jump timeout
                if (_jumpTimeoutDelta > 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else // đang không grounded
            {
                // nếu đang leo
                if (playerState.isPlayerClimbing)
                {
                    // nhảy khi leo
                    if (_input.jump)
                    {
                        playerState.isPlayerClimbing = false;
                        // cho một lực nhảy nhẹ theo hướng tường (tuỳ gameplay)
                        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity * 0.8f);
                        _input.jump = false;
                    }
                    else
                    {
                        // khi leo mà không nhảy, Gravity không ảnh hưởng (giữ player trên tường)
                        _verticalVelocity = 0f;
                    }

                    // khi leo, không tính fall timeout
                    _fallTimeoutDelta = FallTimeout;
                }
                else // đang rơi tự do
                {
                    // reset jump timeout
                    _jumpTimeoutDelta = JumpTimeout;

                    // giảm fall timeout
                    if (_fallTimeoutDelta > 0.0f)
                        _fallTimeoutDelta -= Time.deltaTime;

                    // không cho nhảy nếu không grounded
                    _input.jump = false;

                    // áp dụng gravity
                    if (_verticalVelocity < _terminalVelocity)
                        _verticalVelocity += Gravity * Time.deltaTime;
                }
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
                lfAngle += 360f;
            if (lfAngle > 360f)
                lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded)
                Gizmos.color = transparentGreen;
            else
                Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }
    }
}
