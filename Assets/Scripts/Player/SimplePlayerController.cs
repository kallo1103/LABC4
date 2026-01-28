using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple first-person player controller with AudioListener
/// WASD movement, Mouse look
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioListener))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool lockCursor = true;
    
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool cursorLocked = true;
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Find or create camera
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            
            if (playerCamera == null)
            {
                // Create camera as child
                GameObject camObj = new GameObject("PlayerCamera");
                camObj.transform.SetParent(transform);
                camObj.transform.localPosition = new Vector3(0, 0.6f, 0);
                camObj.transform.localRotation = Quaternion.identity;
                playerCamera = camObj.AddComponent<Camera>();
            }
        }
        
        // Ensure AudioListener is on this object, not camera
        AudioListener listener = GetComponent<AudioListener>();
        if (listener == null)
        {
            gameObject.AddComponent<AudioListener>();
        }
        
        // Remove AudioListener from camera if exists
        AudioListener camListener = playerCamera.GetComponent<AudioListener>();
        if (camListener != null)
        {
            Destroy(camListener);
        }
    }
    
    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorLocked = true;
        }
    }
    
    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleCursorToggle();
    }
    
    private void HandleMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Ground check
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Get input using New Input System
        float x = 0f;
        float z = 0f;
        
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        
        // Calculate movement direction
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Apply movement
        controller.Move(move * moveSpeed * Time.deltaTime);
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    private void HandleMouseLook()
    {
        if (!cursorLocked) return;
        
        var mouse = Mouse.current;
        if (mouse == null) return;
        
        // Get mouse delta
        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;
        
        if (invertY) mouseY = -mouseY;
        
        // Vertical rotation (camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        // Horizontal rotation (player)
        transform.Rotate(Vector3.up * mouseX);
    }
    
    private void HandleCursorToggle()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Toggle cursor lock with Escape
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            cursorLocked = !cursorLocked;
            
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    
    // Public methods for external control
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetMouseSensitivity(float sensitivity) => mouseSensitivity = sensitivity;
    public void LockCursor(bool locked)
    {
        cursorLocked = locked;
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
