using _Project.Scripts.Strategy;
using UnityEngine;
using UnityEngine.InputSystem;

// se asigna a la camara del jugador en este caso!
public class PlayerInteractor : MonoBehaviour
{
    // El "?." de C# no detecta objetos destruidos por Unity (Destroy), esto sí.
    private static bool IsAlive(object o) => o is Object obj && obj != null;
    
    [SerializeField] private float range = 50f;

    // Solo se cachea el foco: para detectar "empecé/dejé de mirar" hay que recordar el frame anterior.
    private IFocusable _currentFocus;
    private InputAction _interactAction;

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Player/Interact");
    }

    private void Update()
    {
        IInteractable interactable = null;
        IFocusable focusable = null;

        // Lanzamos un rayo desde la cámara hacia donde estás mirando.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
        {
            interactable = hit.collider.GetComponentInParent<IInteractable>();
            focusable = hit.collider.GetComponentInParent<IFocusable>();
        }

        UpdateFocus(focusable);

        if (_interactAction.WasPressedThisFrame())
        {
            interactable?.Interact();
        }
    }

    private void UpdateFocus(IFocusable focusable)
    {
        if (focusable == _currentFocus) return;

        if (IsAlive(_currentFocus)) _currentFocus.OnLoseFocus();
        focusable?.OnFocus();
        _currentFocus = focusable;
    }
}
