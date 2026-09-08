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
            Debug.Log($"[KeyObject] E presionado mirando la llave de {_zoneId}, publico ObjectFound"); // debug: sacar despues
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
            // debug: sacar este log despues. Si nunca aparece, el rayo no
            // esta llegando a nada (distancia corta, sin collider en el medio).
            // Si aparece pero con un nombre que no es este GameObject, el
            // Collider esta en otro objeto (por ejemplo un hijo con el mesh)
            // y por eso "hit.transform == transform" da siempre false.
            Debug.Log($"[KeyObject] Rayo pego con: {hit.transform.name} (este objeto se llama: {name})");

            // IsChildOf tambien devuelve true si "hit.transform" es este mismo
            // transform, asi que cubre tanto "collider en este objeto" como
            // "collider en un hijo" (ej. el mesh visual con su propio collider).
            return hit.transform.IsChildOf(transform);
        }
        return false;
    }
}
