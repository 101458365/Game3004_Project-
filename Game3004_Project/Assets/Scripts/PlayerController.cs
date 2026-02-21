using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -19.62f;

    [Header("Look")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.15f;  // PC only;
    [SerializeField] private float lookJoystickSensitivity = 200f;   // mobile (degrees/sec);
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Mobile — Joystick Pack")]
    [SerializeField] private Joystick moveJoystick;   // FixedJoystick bottom-left;
    [SerializeField] private Joystick lookJoystick;   // FixedJoystick bottom-right;

    // this is Raw input (PC / gamepad only);
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;

    private CharacterController cc;
    private Vector3 velocity;
    private float xRotation = 0f;

    // ---- Platform flag ----
    // True when running on mobile OR inside the Unity Device Simulator.
    // On mobile:    Application.isMobilePlatform is true at runtime.
    // In simulator: SystemInfo.deviceType returns DeviceType.Handheld in play mode.
    //               We also check for the simulator via the scripting define
    //               UNITY_ANDROID or UNITY_IOS so mouse look is killed in-editor too
    //               when you switch the active build target to Android/iOS.
    private bool IsMobileOrSimulator
    {
        get
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;   // real device OR Device Simulator with Android/iOS target
#else
            return false;  // PC editor / standalone
#endif
        }
    }

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        // only lock cursor on real PC builds — not on mobile or simulator;
        if (!IsMobileOrSimulator)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

    public void OnLook(InputValue value)
    {
        // i should discard mouse delta entirely on mobile / simulator;
        // the camera look is handled exclusively by the look joystick there;
        if (IsMobileOrSimulator) return;
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value) => isRunning = value.isPressed;

    public void OnJump(InputValue value)
    {
        if (value.isPressed && cc.isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    private void HandleMovement()
    {
        Vector2 input;

        if (moveJoystick != null &&
            (Mathf.Abs(moveJoystick.Horizontal) > 0.01f || Mathf.Abs(moveJoystick.Vertical) > 0.01f))
        {
            input = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
        }
        else
        {
            input = moveInput;
        }

        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        cc.Move(move * speed * Time.deltaTime);

        if (cc.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        float rotX;
        float rotY;

        bool joystickActive = lookJoystick != null &&
            (Mathf.Abs(lookJoystick.Horizontal) > 0.01f || Mathf.Abs(lookJoystick.Vertical) > 0.01f);

        if (joystickActive)
        {
            // Joystick: continuous direction, scaled by deltaTime for frame-rate independence;
            rotX = lookJoystick.Vertical * lookJoystickSensitivity * Time.deltaTime;
            rotY = lookJoystick.Horizontal * lookJoystickSensitivity * Time.deltaTime;
        }
        else if (!IsMobileOrSimulator)
        {
            // if PC mouse delta — only on non-mobile platforms;
            rotX = lookInput.y * mouseSensitivity;
            rotY = lookInput.x * mouseSensitivity;
        }
        else
        {
            // if Mobile / simulator with no joystick input — do nothing;
            return;
        }

        // Pitch — camera only;
        xRotation -= rotX;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw — whole body;
        transform.Rotate(Vector3.up * rotY);
    }

    // PUBLIC API — called by MobileUIButtons;

    public void SetSprinting(bool sprinting) => isRunning = sprinting;

    public void OnJumpButton()
    {
        if (cc.isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }
}