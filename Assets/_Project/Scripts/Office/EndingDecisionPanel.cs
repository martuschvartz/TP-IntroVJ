using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Va en el EndingCanvas (el panel que abre FinalCaseDocument).
// Le da la estetica al cartel final desde codigo, asi no hay que acomodar
// la UI a mano en la escena:
// - fondo oscuro que tapa el despacho,
// - titulo chico arriba + el texto de la reflexion centrado y legible,
// - los dos botones grandes, lado a lado, con hover,
// - y todo entra con un fade.
[RequireComponent(typeof(Canvas))]
public class EndingDecisionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrative;
    [SerializeField] private Button confessButton;
    [SerializeField] private Button noConfessButton;

    [Header("Estilo")]
    [SerializeField] private string title = "MI CASO";
    [SerializeField] private float fadeDuration = 1.2f;
    [SerializeField] private Color backdropColor = new Color(0.02f, 0.02f, 0.03f, 0.92f);
    [SerializeField] private Color textColor = new Color(0.92f, 0.89f, 0.83f, 1f);
    [SerializeField] private Color confessColor = new Color(0.55f, 0.09f, 0.09f, 1f);
    [SerializeField] private Color noConfessColor = new Color(0.16f, 0.17f, 0.2f, 1f);

    private CanvasGroup _group;
    private bool _built;

    private void Awake() => Build();

    private void OnEnable()
    {
        Build();
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private void Build()
    {
        if (_built) return;
        _built = true;

        // Overlay: no depende de la camara del jugador ni se mete adentro de la geometria.
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();

        CreateBackdrop();
        StyleNarrative();
        CreateTitle();
        CreateDivider();

        StyleButton(confessButton, "CONFESAR", confessColor, new Vector2(-230f, -380f));
        StyleButton(noConfessButton, "NO CONFESAR", noConfessColor, new Vector2(230f, -380f));
    }

    private void CreateBackdrop()
    {
        var bg = NewChild("Backdrop");
        bg.SetAsFirstSibling();
        Stretch(bg);
        var img = bg.gameObject.AddComponent<Image>();
        img.color = backdropColor;
        img.raycastTarget = true; // bloquea clicks al mundo de atras
    }

    private void StyleNarrative()
    {
        if (narrative == null) return;
        var rt = narrative.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 40f);
        rt.sizeDelta = new Vector2(1150f, 620f);

        narrative.color = textColor;
        narrative.alignment = TextAlignmentOptions.Center;
        narrative.textWrappingMode = TextWrappingModes.Normal;
        narrative.enableAutoSizing = true;
        narrative.fontSizeMin = 22f;
        narrative.fontSizeMax = 34f;
        narrative.lineSpacing = 12f;
        narrative.paragraphSpacing = 6f;
        narrative.raycastTarget = false;
    }

    private void CreateTitle()
    {
        var rt = NewChild("Title");
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 420f);
        rt.sizeDelta = new Vector2(800f, 60f);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (narrative != null) tmp.font = narrative.font;
        tmp.text = title;
        tmp.fontSize = 26f;
        tmp.characterSpacing = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(textColor.r, textColor.g, textColor.b, 0.55f);
        tmp.raycastTarget = false;
    }

    private void CreateDivider()
    {
        var rt = NewChild("Divider");
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -290f);
        rt.sizeDelta = new Vector2(420f, 2f);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(textColor.r, textColor.g, textColor.b, 0.25f);
        img.raycastTarget = false;
    }

    private void StyleButton(Button button, string label, Color color, Vector2 position)
    {
        if (button == null) return;

        var rt = (RectTransform)button.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(380f, 86f);
        rt.SetAsLastSibling();

        var img = button.GetComponent<Image>();
        if (img != null) img.color = color;

        // El tinte del Button multiplica el color de la Image:
        // en reposo un poco apagado, con hover a full, apretado mas oscuro.
        var colors = button.colors;
        colors.normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.highlightedColor = Color.white;
        colors.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.pressedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;

        // Borde finito claro.
        var outline = button.GetComponent<Outline>();
        if (outline == null) outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(textColor.r, textColor.g, textColor.b, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);

        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.text = label;
            text.color = textColor;
            text.fontSize = 30f;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 12f;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = false;
        }

        if (button.GetComponent<UIHoverScale>() == null)
            button.gameObject.AddComponent<UIHoverScale>();
    }

    private IEnumerator FadeIn()
    {
        _group.alpha = 0f;
        _group.interactable = false;
        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            _group.alpha = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            yield return null;
        }
        _group.alpha = 1f;
        _group.interactable = true;
    }

    private RectTransform NewChild(string childName)
    {
        var go = new GameObject(childName, typeof(RectTransform));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        return rt;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
