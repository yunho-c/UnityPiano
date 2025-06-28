using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed at which the camera moves forward, backward, left, and right.")]
    public float movementSpeed = 5f;
    [Tooltip("Speed at which the camera moves up and down.")]
    public float verticalSpeed = 3f;

    [Header("Rotation Settings")]
    [Tooltip("Sensitivity of the mouse for looking around.")]
    public float mouseSensitivity = 0.1f;

    // Input Action references
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction elevateAction;
    private InputAction enableEDebugAction;

    private bool isDebugEnabled = false;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float elevateInput;

    private float _rotationX = 0f;
    private float _rotationY = 0f;


    // Callback functions to be called by the PlayerInput component
    public void OnEnableDebug(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isDebugEnabled = true;
        }
        else if (context.canceled)
        {
            isDebugEnabled = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnElevate(InputAction.CallbackContext context)
    {
        elevateInput = context.ReadValue<float>();
    }

    public void OnPrintose(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PrintCameraPose();
        }
    }

    void Update()
    {
        if (isDebugEnabled)
        {
            HandleRotation();
            HandleMovement();
        }
    }

    private void HandleRotation()
    {
        _rotationX += lookInput.x * mouseSensitivity;
        _rotationY -= lookInput.y * mouseSensitivity;
        _rotationY = Mathf.Clamp(_rotationY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_rotationY, _rotationX, 0f);
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 elevateDirection = new Vector3(0, elevateInput, 0);

        transform.Translate(moveDirection * movementSpeed * Time.deltaTime);
        transform.Translate(elevateDirection * verticalSpeed * Time.deltaTime, Space.World);
    }

    private void PrintCameraPose()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        string poseMessage = $"Camera Pose:\nPosition: new Vector3({position.x:F3}f, {position.y:F3}f, {position.z:F3}f)\nRotation: new Quaternion({rotation.x:F3}f, {rotation.y:F3}f, {rotation.z:F3}f, {rotation.w:F3}f)";
        Debug.Log(poseMessage);
    }
}
