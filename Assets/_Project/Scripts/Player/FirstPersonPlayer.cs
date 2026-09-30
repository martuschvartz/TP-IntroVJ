using UnityEngine;
using UnityEngine.Serialization;

// Controlador de jugador super minimo: caminar (WASD) y girar (mouse).
// Nada de salto, sprint ni gravedad especial. Sirve para poder probar
// KeyObject y el recorrido de habitaciones caminando en primera persona.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayer : MonoBehaviour
{
    private bool CanMove { get; } = true;
    
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float gravity = 500f;
    
    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity = 1200f;
    [SerializeField] private float upperLookLimit = 90f;
    [SerializeField] private float lowerLookLimit = 90f;
    
    private Camera _playerCamera;
    private CharacterController _controller;
    
    private Vector3 _moveDirection;
    private Vector2 _moveInput;
    
    private float _xRotation;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (CanMove)
        {
            HandleLook();
            HandleMove();
            
            ApplyFinalMovement();
        }
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -lowerLookLimit, upperLookLimit);
        
        _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0, mouseX, 0f);
    }

    private void HandleMove()
    {
        float horizontal = Input.GetAxis("Horizontal") * moveSpeed;
        float vertical = Input.GetAxis("Vertical") * moveSpeed;
        
        float directionY = _moveDirection.y;
        _moveDirection = transform.TransformDirection(Vector3.forward) * vertical + transform.TransformDirection(Vector3.right) * horizontal;
        _moveDirection.y = directionY;
    }

    private void ApplyFinalMovement()
    {
        if (!_controller.isGrounded)
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }
        _controller.Move(_moveDirection * Time.deltaTime);
    }
}
