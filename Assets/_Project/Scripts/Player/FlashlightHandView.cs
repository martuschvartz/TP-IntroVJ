using UnityEngine;

// Parte visual de "agarrar la llave": cuando se encuentra el objeto clave de
// una zona, oculta la llave que estaba sobre la mesa y muestra la llave que
// el jugador lleva en la mano.
//
// Igual que RoomController, este componente NO decide cuando pasa esto: solo
// reacciona al evento ObjectFound (el mismo que dispara la barra espaciadora
// en el debug de RoomManager, o la tecla E mirando la KeyObject). Asi la parte
// visual y la logica de habitaciones quedan desacopladas.
public class FlashlightHandView : MonoBehaviour
{
    [Tooltip("Zona a la que pertenece esta llave. Solo reacciona si se encuentra el objeto de ESTA zona.")]
    [SerializeField] private ZoneId _zoneId;

    [Tooltip("Llave apoyada en la mesa. Se oculta al encontrarla.")]
    [SerializeField] private GameObject _flashlightOnTable;

    [Tooltip("Llave en la mano del jugador. Se muestra al encontrarla.")]
    [SerializeField] private GameObject _flashlightInHand;

    private void OnEnable()
    {
        GameEvents.FlashlightFound += HandleObjectFound;
    }

    private void OnDisable()
    {
        GameEvents.FlashlightFound -= HandleObjectFound;
    }

    private void Start()
    {
        // Estado inicial conocido: la llave esta en la mesa, no en la mano.
        SetFlashlightPickedUp(false);
    }

    private void HandleObjectFound(ZoneId zone)
    {
        if (zone != _zoneId) return;

        SetFlashlightPickedUp(true);
    }

    private void SetFlashlightPickedUp(bool pickedUp)
    {
        if (_flashlightOnTable != null) _flashlightOnTable.SetActive(!pickedUp);
        if (_flashlightInHand != null) _flashlightInHand.SetActive(pickedUp);
    }
}
