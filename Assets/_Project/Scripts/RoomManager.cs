using UnityEngine;

// Coordina QUE zona esta activa y CUANDO pasar a la siguiente.
// No conoce al timer ni al KeyObject directamente: solo reacciona al
// evento ObjectFound. Esto permite probar este script solo, sin timer
// ni objetos interactuables en la escena (ver el debug comentado abajo).
public class RoomManager : MonoBehaviour
{
    // Orden del recorrido: Living, Patio, Bosque (ese orden importa).
    [SerializeField] private RoomController[] _rooms;

    private int _currentIndex;

    #region Suscripcion a eventos
    
    // Se suscribe al evento ObjectFound. Cuando alguien llame a raiseObjectFound, se va a correr
    // HandleObjectFound con el zoneId (y todos los métodos que se agreguen con +=).
    private void OnEnable()
    {
        GameEvents.ObjectFound += HandleObjectFound;
    }

    private void OnDisable()
    {
        GameEvents.ObjectFound -= HandleObjectFound;
    }
    #endregion

    private void Start()
    {
        // Arrancamos siempre desde un estado conocido: todo oculto,
        // y despues revelamos solo la primera zona.
        HideAll();
        _currentIndex = 0;
        _rooms[_currentIndex].Reveal();
    }

    private void HandleObjectFound(ZoneId zone)
    {
        // Solo nos importa el objeto de la zona activa. Si llegara el de
        // otra zona (no deberia pasar en este recorrido lineal) lo ignoramos.
        if (zone != _rooms[_currentIndex].ZoneId) return;

        _currentIndex++;
        if (_currentIndex < _rooms.Length)
        {
            _rooms[_currentIndex].Reveal();
        }
        else
        {
            // No queda una zona siguiente: se completo todo el recorrido.
            GameEvents.RaiseGameWon();
        }
    }

    // Se puede usar mas adelante para ocultar todo de una, por ejemplo
    // cuando agregues el timer y se acabe el tiempo.
    public void HideAll()
    {
        foreach (RoomController room in _rooms)
        {
            room.Hide();
        }
    }

    #region Debug: probar el sistema de habitaciones solo (sin KeyObject)
    // Barra espaciadora = simula que se encontro el objeto clave de la
    // zona actual, sin necesitar el jugador ni el KeyObject en la escena.
    private void Update()
    {
        // Si ya no hay zona activa (se revelo todo), no hacemos nada:
        // sin este chequeo, _rooms[_currentIndex] rompe con index out of range.
        if (_currentIndex >= _rooms.Length) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameEvents.RaiseObjectFound(_rooms[_currentIndex].ZoneId);
        }
    }
    #endregion
}
