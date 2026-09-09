using UnityEngine;

// Controlador de jugador super minimo: caminar (WASD) y girar (mouse).
// Nada de salto, sprint ni gravedad especial. Sirve para poder probar
// KeyObject y el recorrido de habitaciones caminando en primera persona.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayer : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _mouseSensitivity = 200f;

    private CharacterController _controller;
    private float _cameraPitch; // rotacion acumulada de la camara en el eje X (arriba/abajo)

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        // El cuerpo gira en Y (izquierda/derecha)...
        transform.Rotate(Vector3.up, mouseX);

        // ...y la camara en X (arriba/abajo), con limite para no dar la vuelta completa.
        _cameraPitch = Mathf.Clamp(_cameraPitch - mouseY, -80f, 80f);
        _playerCamera.transform.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
    }

    private void HandleMove()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D va entre -1 y 1
        float vertical = Input.GetAxis("Vertical");      // W/S va entre -1 y 1

        Vector3 move = (transform.right * horizontal + transform.forward * vertical).normalized;
        _controller.Move(move * (_moveSpeed * Time.deltaTime));
    }
}
