using UnityEngine;

// Objeto clave de una zona. Su unica responsabilidad es detectar que el
// jugador lo esta mirando y aprieta E, y avisar por evento que se encontro.
// No conoce a RoomManager: solo anuncia "me encontraron en esta zona".
[RequireComponent(typeof(Collider))]
public class KeyObject : MonoBehaviour
{
    [SerializeField] private ZoneId _zoneId;
    [SerializeField] private float _interactDistance = 3f;

    private void Update()
    {
        if (!IsBeingLookedAt()) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameEvents.RaiseObjectFound(_zoneId);
        }
    }

    // Interaccion minima: tira un rayo recto desde el centro de la camara
    // del jugador y ve si lo primero que toca es este mismo objeto.
    // Se puede refinar mas adelante (icono de "mira aca", capas propias,
    // tolerancia de angulo, etc.), pero para la base alcanza con esto.
    private bool IsBeingLookedAt()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance))
        {
            return hit.transform.IsChildOf(transform);
        }
        return false;
    }
}
