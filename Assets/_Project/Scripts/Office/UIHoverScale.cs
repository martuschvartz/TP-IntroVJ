using UnityEngine;
using UnityEngine.EventSystems;

// Agranda un poco el elemento de UI cuando el mouse pasa por encima.
// Lo agrega EndingDecisionPanel a los botones del final.
public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float speed = 12f;

    private float _target = 1f;

    public void OnPointerEnter(PointerEventData eventData) => _target = hoverScale;
    public void OnPointerExit(PointerEventData eventData) => _target = 1f;

    private void OnDisable()
    {
        _target = 1f;
        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        float s = Mathf.Lerp(transform.localScale.x, _target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }
}
