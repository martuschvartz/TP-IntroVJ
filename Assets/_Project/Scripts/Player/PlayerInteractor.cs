using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
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
    [Tooltip("Si lo primero que toca el rayo no es interactuable (una mesa, un estante), igual toma una pista " +
             "que este hasta esta distancia detras. Así las botellas al lado de la mesa se pueden agarrar. 0 = desactivado.")]
    [SerializeField] private float lookThroughDistance = 40f;
    [Tooltip("Debug: al apretar E, escribe en la Console contra qué collider chocó el rayo.")]
    [SerializeField] private bool logHits = true;

    // Solo se cachea el foco: para detectar "empecé/dejé de mirar" hay que recordar el frame anterior.
    private IFocusable _currentFocus;
    // Igual que el foco: se recuerda el icono de la mira del frame anterior,
    // para avisarle al HUD solo cuando cambia.
    private CrosshairIcon _currentIcon = CrosshairIcon.Dot;
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
            UpdateCrosshairIcon(CrosshairIcon.Dot);
            return;
        }

        IInteractable interactable = null;
        IFocusable focusable = null;

        RaycastHit hit = default;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, range, interactionMask);
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            hit = hits[0];
            float limit = hits[0].distance + lookThroughDistance;

            // Lo primero que toca, o (si eso no es interactuable, ej: el borde de una mesa)
            // la primera pista que este apenas detras.
            foreach (RaycastHit h in hits)
            {
                if (h.distance > limit) break;
                // GetComponentInParent: el collider puede estar en un hijo del objeto que tiene el script.
                IInteractable i = h.collider.GetComponentInParent<IInteractable>();
                if (i == null) continue;
                hit = h;
                interactable = i;
                focusable = h.collider.GetComponentInParent<IFocusable>();
                break;
            }
        }

        UpdateFocus(focusable);
        // Cada interactuable dice su icono; si no hay ninguno, puntito.
        UpdateCrosshairIcon(interactable?.Icon ?? CrosshairIcon.Dot);

        if (_interactAction.WasPressedThisFrame())
        {
            if (logHits) LogHit(hit, interactable);
            interactable?.Interact();
        }
    }

    // Para entender por que un objeto "no toma" la E: muestra que tocó el rayo
    // y si ese collider pertenece a algo interactuable. Hacé click en el log y
    // Unity te marca el objeto en la Hierarchy.
    private static void LogHit(RaycastHit hit, IInteractable interactable)
    {
        if (hit.collider == null)
        {
            Debug.Log("[Interactor] El rayo no tocó nada.");
            return;
        }

        string target = interactable != null ? ((Component)interactable).name : "NADA interactuable";
        Debug.Log($"[Interactor] Tocó el collider de '{hit.collider.name}' -> {target}", hit.collider);
    }

    private void UpdateCrosshairIcon(CrosshairIcon icon)
    {
        if (icon == _currentIcon) return;

        _currentIcon = icon;
        GameEvents.RaiseCrosshairIconChanged(icon);
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
