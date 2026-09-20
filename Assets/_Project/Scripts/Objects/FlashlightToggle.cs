using UnityEngine;

// Prende y apaga la luz de la linterna con la barra espaciadora.
//
// Va en el GameObject de la linterna EN LA MANO. Como ese objeto arranca
// desactivado y recien se activa cuando FlashlightHandView recibe el evento,
// este Update() no corre (ni responde al espacio) hasta que el jugador
// efectivamente tiene la linterna. No necesita saber nada de los eventos.
public class FlashlightToggle : MonoBehaviour
{
    [Tooltip("Luz de la linterna (normalmente un Spot Light hijo). Si se deja vacio se busca en los hijos.")]
    [SerializeField] private Light _light;

    [SerializeField] private KeyCode _toggleKey = KeyCode.Space;

    [Tooltip("Si la linterna arranca prendida al agarrarla.")]
    [SerializeField] private bool _startsOn = false;

    private void Awake()
    {
        if (_light == null) _light = GetComponentInChildren<Light>(includeInactive: true);
    }

    private void OnEnable()
    {
        // debug: sacar despues. Si esto no aparece al agarrar la linterna,
        // el GameObject de este script nunca se activa (FlashlightHandView
        // no lo activa, o el script esta en otro objeto).
        Debug.Log($"[FlashlightToggle] OnEnable en '{name}', _light asignada: {_light != null}");

        // Cada vez que se agarra la linterna, arranca en un estado conocido.
        if (_light != null) _light.enabled = _startsOn;
    }

    private void Update()
    {
        if (_light == null) return;

        if (Input.GetKeyDown(_toggleKey))
        {
            _light.enabled = !_light.enabled;
            Debug.Log($"[FlashlightToggle] {_toggleKey} -> luz {( _light.enabled ? "PRENDIDA" : "apagada")}"); // debug: sacar despues
        }
    }
}
