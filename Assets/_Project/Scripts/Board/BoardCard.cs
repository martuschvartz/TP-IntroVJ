using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Una tarjeta del tablero: una pista (ClueData) o una declaracion (StatementData).
// Se arrastra con el mouse; un click la elige como extremo de un hilo.
// Si su sospechoso ya se cerro, queda fija: no se mueve ni se conecta.
[RequireComponent(typeof(RectTransform))]
public class BoardCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text label;
    [Tooltip("La imagen de fondo de la tarjeta (se tiñe al seleccionarla).")]
    [SerializeField] private Image background;

    private BoardUI _board;
    private Color _baseColor;
    private Color _selectedColor;
    private Vector2 _dragOffset;

    public ScriptableObject Data { get; private set; }
    public ClueData Clue => Data as ClueData;
    public StatementData Statement => Data as StatementData;
    public bool IsClue => Clue != null;
    // De quien es la tarjeta (para saber si su caso ya se cerro).
    public string SuspectId => IsClue ? Clue.SuspectId : Statement != null ? Statement.SuspectId : null;
    public RectTransform Rect { get; private set; }

    public void Init(BoardUI board, ScriptableObject data, string text, Color baseColor, Color selectedColor)
    {
        _board = board;
        Data = data;
        Rect = (RectTransform)transform;
        label.text = text;
        _baseColor = baseColor;
        _selectedColor = selectedColor;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? _selectedColor : _baseColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Si se estaba arrastrando, no cuenta como click.
        if (!_board.CanEdit(this) || eventData.dragging || eventData.button != PointerEventData.InputButton.Left) return;
        _board.OnCardClicked(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_board.CanEdit(this) || !TryGetPointer(eventData, out Vector2 pointer)) return;
        // Se guarda donde se agarro la tarjeta, para que no "salte" al centro del mouse.
        _dragOffset = (Vector2)Rect.localPosition - pointer;
        // Que la tarjeta que se arrastra quede por encima de las demas.
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_board.CanEdit(this) || !TryGetPointer(eventData, out Vector2 pointer)) return;
        Vector2 target = ClampInsideArea(pointer + _dragOffset);
        Rect.localPosition = new Vector3(target.x, target.y, Rect.localPosition.z);
    }

    // El mouse en coordenadas del corcho. El canvas es World Space, asi que hay que
    // pasar por la camara que esta mirando (no alcanza con mover en pixeles).
    private bool TryGetPointer(PointerEventData eventData, out Vector2 pointer)
    {
        RectTransform parent = (RectTransform)Rect.parent;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out pointer);
    }

    // Que la tarjeta no se pueda sacar de su zona (o del corcho, si su sospechoso no tiene zona).
    private Vector2 ClampInsideArea(Vector2 position)
    {
        Rect area = _board.DragArea(this);
        Vector2 half = Rect.rect.size / 2f;
        position.x = Mathf.Clamp(position.x, area.xMin + half.x, area.xMax - half.x);
        position.y = Mathf.Clamp(position.y, area.yMin + half.y, area.yMax - half.y);
        return position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_board.CanEdit(this)) return;
        GameProgress.SetCardPosition(Data, Rect.anchoredPosition);
    }
}
