using UnityEngine;

// Parte visual de "agarrar la llave": cuando se encuentra el objeto clave de
// una zona, oculta la llave que estaba sobre la mesa y muestra la llave que
// el jugador lleva en la mano.
//
// Igual que RoomController, este componente NO decide cuando pasa esto: solo
// reacciona al evento ObjectFound (el mismo que dispara la barra espaciadora
// en el debug de RoomManager, o la tecla E mirando la KeyObject). Asi la parte
// visual y la logica de habitaciones quedan desacopladas.
public class KeyHandView : MonoBehaviour
{
    [Tooltip("Zona a la que pertenece esta llave. Solo reacciona si se encuentra el objeto de ESTA zona.")]
    [SerializeField] private ZoneId _zoneId;

    [Tooltip("Llave apoyada en la mesa. Se oculta al encontrarla.")]
    [SerializeField] private GameObject _keyOnTable;

    [Tooltip("Llave en la mano del jugador. Se muestra al encontrarla.")]
    [SerializeField] private GameObject _keyInHand;

    private void OnEnable()
    {
        GameEvents.KeyFound += HandleObjectFound;
    }

    private void OnDisable()
    {
        GameEvents.KeyFound -= HandleObjectFound;
    }

    private void Start()
    {
        // Estado inicial conocido: la llave esta en la mesa, no en la mano.
        SetKeyPickedUp(false);
    }

    private void HandleObjectFound(ZoneId zone)
    {
        if (zone != _zoneId) return;

        SetKeyPickedUp(true);
    }

    private void SetKeyPickedUp(bool pickedUp)
    {
        if (_keyOnTable != null) _keyOnTable.SetActive(!pickedUp);
        if (_keyInHand != null) _keyInHand.SetActive(pickedUp);
    }
}
