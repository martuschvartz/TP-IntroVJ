using UnityEngine;

// Una habitacion (zona) solo sabe COMO mostrarse u ocultarse.
// A proposito NO decide cuando hacerlo ni que orden siguen las zonas:
// esa decision es de RoomManager. Este componente se puede probar solo
// llamando Reveal()/Hide() a mano desde el Inspector o desde un test.
public class RoomController : MonoBehaviour
{
    [SerializeField] private ZoneId _zoneId;

    public ZoneId ZoneId => _zoneId;

    public void Reveal()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
