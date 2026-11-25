using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Un hilo entre dos tarjetas. Es una Image finita que se estira y se rota
// para ir de una tarjeta a la otra; se reacomoda sola si las tarjetas se mueven.
// Click derecho sobre el hilo lo borra (solo en modo edicion).
[RequireComponent(typeof(RectTransform), typeof(Image))]
public class BoardThread : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float thickness = 6f;

    private BoardUI _board;
    private BoardCard _clueCard;
    private BoardCard _statementCard;
    private RectTransform _rect;
    private RectTransform _layer;

    public void Init(BoardUI board, BoardCard clueCard, BoardCard statementCard, Color color)
    {
        _board = board;
        _clueCard = clueCard;
        _statementCard = statementCard;
        _rect = (RectTransform)transform;
        _layer = (RectTransform)transform.parent;
        GetComponent<Image>().color = color;

        // Anclado y con pivot al centro, para poder ubicarlo por su punto medio.
        _rect.anchorMin = _rect.anchorMax = _rect.pivot = new Vector2(0.5f, 0.5f);
        Follow();
    }

    private void LateUpdate()
    {
        Follow();
    }

    private void Follow()
    {
        // Las posiciones de las tarjetas pasadas al espacio de la capa de hilos.
        Vector2 a = _layer.InverseTransformPoint(_clueCard.Rect.position);
        Vector2 b = _layer.InverseTransformPoint(_statementCard.Rect.position);
        Vector2 delta = b - a;

        _rect.localPosition = (a + b) / 2f;
        _rect.sizeDelta = new Vector2(delta.magnitude, thickness);
        _rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_board.CanEdit(_clueCard) || !_board.CanEdit(_statementCard) || eventData.button != PointerEventData.InputButton.Right) return;
        _board.RemoveThread(_clueCard.Clue, _statementCard.Statement);
    }
}
