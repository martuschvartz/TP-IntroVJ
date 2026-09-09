using UnityEngine;

// Objeto linterna de una zona. Detecta que el jugador lo esta mirando
// y aprieta E, y avisa por el evento especifico de la linterna.
[RequireComponent(typeof(Collider))]
public class FlashlightObject : MonoBehaviour
{
    [SerializeField] private ZoneId _zoneId;
    [SerializeField] private float _interactDistance = 3f;

    private void Update()
    {
        if (!IsBeingLookedAt()) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[FlashlightObject] E presionado mirando la linterna de {_zoneId}, publico FlashlightFound");
            GameEvents.RaiseFlashlightFound(_zoneId);
        }
    }

    private bool IsBeingLookedAt()
    {
        if (Camera.main == null) return false;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance))
        {
            Debug.Log($"[FlashlightObject] Rayo pego con: {hit.transform.name} (este objeto se llama: {name})");
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }

        return false;
    }
}
