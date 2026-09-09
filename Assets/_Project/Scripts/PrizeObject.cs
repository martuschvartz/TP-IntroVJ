using UnityEngine;

// Objeto regalo de una zona (el bosque). Detecta que el jugador lo esta
// mirando y aprieta E, y avisa por el evento especifico del regalo.
[RequireComponent(typeof(Collider))]
public class PrizeObject : MonoBehaviour
{
    [SerializeField] private ZoneId _zoneId;
    [SerializeField] private float _interactDistance = 3f;

    private void Update()
    {
        if (!IsBeingLookedAt()) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[PrizeObject] E presionado mirando el regalo de {_zoneId}, publico PrizeFound");
            GameEvents.RaisePrizeFound(_zoneId);
        }
    }

    private bool IsBeingLookedAt()
    {
        if (Camera.main == null) return false;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }

        return false;
    }
}
