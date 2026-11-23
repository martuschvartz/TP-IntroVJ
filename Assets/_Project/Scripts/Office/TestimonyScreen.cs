using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pantalla negra a pantalla completa con el resumen del testimonio de un
// sospechoso. Se arma por codigo (Canvas + fondo negro + textos) para no
// tener que agregar UI a mano en el despacho: la crea MemoryDocument justo
// antes de cargar el recuerdo, y se destruye sola al cambiar de escena.
public static class TestimonyScreen
{
    public static TMP_Text Show(string title, string testimony, out TMP_Text hint)
    {
        GameObject canvasObject = new GameObject("TestimonyScreen", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // por encima de cualquier otra UI del despacho

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject background = new GameObject("Background", typeof(Image));
        background.transform.SetParent(canvasObject.transform, false);
        background.GetComponent<Image>().color = Color.black;
        Stretch(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        CreateLabel(canvasObject.transform, title, 56, FontStyles.Bold, new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.88f));
        TMP_Text body = CreateLabel(canvasObject.transform, testimony, 38, FontStyles.Italic, new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.72f));
        hint = CreateLabel(canvasObject.transform, "", 26, FontStyles.Normal, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.15f));
        hint.color = new Color(1f, 1f, 1f, 0.6f);

        return body;
    }

    private static TMP_Text CreateLabel(Transform parent, string text, float size, FontStyles style, Vector2 min, Vector2 max)
    {
        GameObject labelObject = new GameObject("Label", typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);
        Stretch(labelObject.GetComponent<RectTransform>(), min, max);

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }

    // Hace que el RectTransform ocupe la porcion de pantalla [min, max] (en 0..1).
    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
