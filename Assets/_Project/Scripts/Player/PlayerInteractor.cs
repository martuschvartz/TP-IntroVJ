using _Project.Scripts.Strategy;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * RAYCASTING
 *
 * Va en la cámara del jugador. Cada frame tira un rayo invisible desde la
 * cámara hacia adelante (el centro de la pantalla) para saber qué está mirando:
 *
 *  - Origen:    transform.position  (la cámara)
 *  - Dirección: transform.forward   (hacia donde mira)
 *  - Largo:     range               (se ajusta por escena en el Inspector,
 *                                    porque las escenas tienen escalas distintas)
 *  - Máscara:   interactionMask     (qué layers puede tocar el rayo)
 *
 * El rayo se frena en el PRIMER collider que toca. Si ese objeto (o alguno de
 * sus padres) implementa:
 *  - IFocusable    -> se prende su highlight mientras lo mirás.
 *  - IInteractable -> al apretar Interact (E) se llama a su Interact().
 *
 * PlayerInteractor no sabe si lo que mira es una pista, un documento o una
 * puerta: solo habla con las interfaces (patrón Strategy).
 */
public class PlayerInteractor : MonoBehaviour
{
    // El "?." de C# no detecta objetos destruidos por Unity (Destroy), esto sí.
    private static bool IsAlive(object o) => o is Object obj && obj != null;
    
    [SerializeField] private float range = 100f;
    
    [SerializeField] private LayerMask interactionMask = ~0; // ~0 = todas las layers

    // Solo se cachea el foco: para detectar "empecé/dejé de mirar" hay que recordar el frame anterior.
    private IFocusable _currentFocus;
    private InputAction _interactAction;
    // Mientras el jugador esta bloqueado (cinematica, tablero abierto...) no se interactua:
    // si no, apretar E con el tablero abierto activaria lo que quedo en el centro de la pantalla.
    private bool _locked;

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Player/Interact");
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
        _locked = locked;
    }

    private void Update()
    {
        if (_locked)
        {
            UpdateFocus(null);
            return;
        }

        IInteractable interactable = null;
        IFocusable focusable = null;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range, interactionMask))
        {
            // GetComponentInParent: el collider puede estar en un hijo del objeto que tiene el script.
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

#if UNITY_EDITOR
    // Dibuja el rayo en la vista Scene (solo en el editor) para poder verlo en la demo.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * range);
    }
#endif
}
