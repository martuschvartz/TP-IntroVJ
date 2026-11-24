using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Muestra el guion del cuarto paralelo como subtitulos semitransparentes,
// para seguir viendo la escena mientras se lee (no hay audio de los dialogos).
// El guion es un .txt en Resources/Guiones/ con el mismo nombre que la escena.
// Cada linea no vacia es un subtitulo:
//   [Acotacion]            -> en cursiva
//   NOMBRE: texto (pausa)  -> nombre en negrita, parentesis en cursiva
// Se arma solo (canvas incluido): VisitParallelSceneCommand lo crea al llegar.
public class ParallelSubtitles : MonoBehaviour
{
    private const string ScriptsFolder = "Guiones/";
    private const int VisibleLines = 3;          // cuantas lineas quedan en pantalla a la vez
    private const float SecondsPerLine = 1.5f;   // minimo por linea
    private const float SecondsPerChar = 0.06f;  // ~16 caracteres por segundo
    private const float NewestAlpha = 0.7f;      // la linea actual
    private const float OldestAlpha = 0.3f;      // las anteriores se van apagando

    private static readonly Regex SpeakerRegex = new Regex(@"^([A-ZÁÉÍÓÚÑ ]+):\s*(.*)$");
    private static readonly Regex ParenRegex = new Regex(@"\(([^)]*)\)");

    private readonly List<string> _shown = new List<string>();
    private TMP_Text _label;

    public bool IsPlaying { get; private set; }

    // Devuelve null si la escena no tiene guion.
    public static ParallelSubtitles TryStart(string sceneName)
    {
        TextAsset script = Resources.Load<TextAsset>(ScriptsFolder + sceneName);
        if (script == null) return null;

        ParallelSubtitles subtitles = new GameObject("ParallelSubtitles").AddComponent<ParallelSubtitles>();
        subtitles.BuildCanvas();
        subtitles.StartCoroutine(subtitles.Play(script.text));
        return subtitles;
    }

    private void BuildCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(transform, false);
        RectTransform rect = (RectTransform)textObject.transform;
        rect.anchorMin = new Vector2(0.15f, 0.04f);
        rect.anchorMax = new Vector2(0.85f, 0.4f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _label = textObject.AddComponent<TextMeshProUGUI>();
        _label.fontSize = 30;
        _label.alignment = TextAlignmentOptions.Bottom;
        _label.color = Color.white;
        _label.raycastTarget = false;
        _label.text = "";
    }

    private IEnumerator Play(string script)
    {
        IsPlaying = true;
        foreach (string rawLine in script.Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.Length == 0) continue;

            _shown.Add(Format(line));
            if (_shown.Count > VisibleLines) _shown.RemoveAt(0);
            Refresh();

            yield return new WaitForSeconds(SecondsPerLine + line.Length * SecondsPerChar);
        }
        _label.text = "";
        IsPlaying = false;
    }

    private void Refresh()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _shown.Count; i++)
        {
            // La mas nueva (ultima) con NewestAlpha; las de arriba mas tenues.
            float t = _shown.Count == 1 ? 1f : (float)i / (_shown.Count - 1);
            float alpha = Mathf.Lerp(OldestAlpha, NewestAlpha, t);
            sb.Append("<alpha=#").Append(Mathf.RoundToInt(alpha * 255).ToString("X2")).Append('>');
            sb.Append(_shown[i]);
            if (i < _shown.Count - 1) sb.Append('\n');
        }
        _label.text = sb.ToString();
    }

    private static string Format(string line)
    {
        if (line.StartsWith("[")) return "<i>" + line.Trim('[', ']') + "</i>";

        string italicParens = ParenRegex.Replace(line, "<i>($1)</i>");
        Match speaker = SpeakerRegex.Match(italicParens);
        if (!speaker.Success) return italicParens;
        return "<b>" + speaker.Groups[1].Value + ":</b> " + speaker.Groups[2].Value;
    }
}
