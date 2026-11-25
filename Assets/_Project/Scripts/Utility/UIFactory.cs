using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Arma UI desde codigo con la misma estetica que el cartel final
// (fondo oscuro, borde finito claro, texto en mayusculas espaciado, hover).
// La usa el boton "Volver a inicio" de los finales (EndingMenuButton).
public static class UIFactory
{
    public static readonly Color TextColor = new Color(0.92f, 0.89f, 0.83f, 1f);
    public static readonly Color DarkColor = new Color(0.08f, 0.08f, 0.1f, 0.85f);

    // Canvas overlay propio, por encima del resto de la UI del juego.
    public static Canvas CreateOverlayCanvas(string name, int sortingOrder, Transform parent = null)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        if (parent != null) go.transform.SetParent(parent, false);

        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static Button CreateButton(Transform parent, string label, Color color, Vector2 size, float fontSize, UnityAction onClick)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(UIHoverScale));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.color = color;

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.highlightedColor = Color.white;
        colors.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.pressedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        if (onClick != null) button.onClick.AddListener(onClick);

        var outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(TextColor.r, TextColor.g, TextColor.b, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);

        var textGo = new GameObject("Text", typeof(RectTransform));
        var textRt = (RectTransform)textGo.transform;
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.color = TextColor;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.characterSpacing = 10f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return button;
    }

    // Los botones necesitan un EventSystem en la escena. Algunas (ej: los finales) no tienen.
    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null || Object.FindAnyObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }
}
