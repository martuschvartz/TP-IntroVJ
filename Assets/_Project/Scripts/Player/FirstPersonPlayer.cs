using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

// Controlador de jugador super minimo: caminar (WASD) y girar (mouse).
// Nada de salto, sprint ni gravedad especial. Sirve para poder probar
// KeyObject y el recorrido de habitaciones caminando en primera persona.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayer : MonoBehaviour
{
    private bool CanMove { get; set; } = true;
    
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float gravity = 500f;
    
    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity = 1200f;
    [SerializeField] private float upperLookLimit = 90f;
    [SerializeField] private float lowerLookLimit = 90f;

    [Tooltip("Lo que gira al mirar arriba/abajo. Con Cinemachine: la CinemachineCamera del jugador. Si queda vacío, usa la Camera hija.")]
    [SerializeField] private Transform lookTarget;

    private CharacterController _controller;
    private InputAction _moveAction;
    private InputAction _lookAction;

    private Vector3 _moveDirection;
    private Vector2 _moveInput;

    private float _xRotation;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (lookTarget == null) lookTarget = GetComponentInChildren<Camera>().transform;
        _moveAction = InputSystem.actions.FindAction("Player/Move");
        _lookAction = InputSystem.actions.FindAction("Player/Look");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        GameEvents.PlayerLockChanged += HandlePlayerLockChanged;
    }

    private void OnDisable()
    {
        GameEvents.PlayerLockChanged -= HandlePlayerLockChanged;
    }

    private void HandlePlayerLockChanged(bool locked)
    {
        CanMove = !locked;
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
        // El 0.1 iguala la escala del Input viejo (Input.GetAxis("Mouse X")), así la sensibilidad se siente igual.
        Vector2 look = _lookAction.ReadValue<Vector2>() * 0.1f;
        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -lowerLookLimit, upperLookLimit);

        lookTarget.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0, mouseX, 0f);
    }

    private void HandleMove()
    {
        Vector2 move = _moveAction.ReadValue<Vector2>();
        float horizontal = move.x * moveSpeed;
        float vertical = move.y * moveSpeed;
        
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
