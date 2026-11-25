using UnityEngine;
using UnityEngine.EventSystems;

// Una zona del corcho: el lugar de un sospechoso (sus declaraciones y sus pistas).
// Se arma en Unity como un rectangulo (Image) dentro del canvas del corcho, DETRAS de
// las capas de hilos y tarjetas (mas arriba en la jerarquia), asi no tapa los clicks.
// - Las tarjetas nuevas del sospechoso aparecen adentro de su zona.
// - Con el tablero abierto, un click en el fondo de la zona acerca la camara a ella.
// - Desde el despacho, apretar E mirando la zona abre el tablero ya acercado ahi.
// "Acercarse" = prender su Zone Camera: una copia de BoardCam acomodada a mano mirando
// esta zona (Align With View). Arranca apagada; la prende y apaga BoardUI.
// La Image puede ser transparente (alfa 0) pero tiene que tener Raycast Target prendido.
[RequireComponent(typeof(RectTransform))]
public class BoardZone : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("De quien es esta zona.")]
    [SerializeField] private SuspectData suspect;
    [Tooltip("La CinemachineCamera que mira esta zona (copia de BoardCam). Vacio = no se acerca, se queda en Ver todo.")]
    [SerializeField] private GameObject zoneCamera;

    private BoardUI _board;

    public SuspectData Suspect => suspect;
    public RectTransform Rect => (RectTransform)transform;
    public GameObject ZoneCamera => zoneCamera;

    public void Init(BoardUI board)
    {
        _board = board;
    }

    // Si un punto del mundo (ej: donde pego el rayo en el corcho) cae adentro de la zona.
    public bool Contains(Vector3 worldPoint)
    {
        Vector3 local = Rect.InverseTransformPoint(worldPoint);
        return Rect.rect.Contains(new Vector2(local.x, local.y));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_board == null || !_board.IsOpen || eventData.button != PointerEventData.InputButton.Left) return;
        _board.FocusZone(this);
    }
}
