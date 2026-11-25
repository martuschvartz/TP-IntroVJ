using System.Collections;
using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Reproduce el fragmento de un sospechoso (la noche vista con sus ojos) cuando se
// cierra su caso desde el tablero con suficientes contradicciones (ej: Living-template).
// Este script se crea solo al cargar la escena (no hay que ponerlo a mano), pasa el
// guion (Resources/Guiones/<nombre>.txt) como subtitulos y despues sigue a la escena
// de "despues" (normalmente el despacho).
// Antes de cargarlo, la pantalla parpadea a negro dos veces (BlinkTransition).
// Abajo a la derecha hay un boton "Omitir" (tambien con Enter) para saltear el fragmento.
public class FragmentPlayer : MonoBehaviour
{
    // Si la escena no tiene guion, se queda al menos este tiempo.
    private const float MinSeconds = 5f;
    private const float SecondsAfterScript = 1.5f;
    private const string SkipLabel = "Omitir  [Enter]";

    private static string _pendingScene;
    private static string _pendingScript;
    private static SceneName _pendingAfter;

    private SceneName _after;
    private string _script;
    private ParallelSubtitles _subtitles;
    private bool _leaving;

    // Lo llama el tablero en vez de cargar la escena directo.
    // script = nombre del guion en Resources/Guiones (vacio = el de la escena).
    public static void Play(SceneName fragment, SceneName after, string script = null)
    {
        _pendingScene = fragment.FileName();
        _pendingScript = string.IsNullOrWhiteSpace(script) ? _pendingScene : script;
        _pendingAfter = after;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Antes de entrar, un "parpadeo" de desconcierto: la pantalla se va a negro
        // y vuelve dos veces, y recien ahi carga el fragmento.
        BlinkTransition.Run(() => SceneAdministrator.LoadWithLoadingScreen(fragment));
    }

    // Antes del fragmento se carga AsyncLoading: se espera a que llegue la escena pedida.
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != _pendingScene) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        _pendingScene = null;

        FragmentPlayer player = new GameObject("FragmentPlayer").AddComponent<FragmentPlayer>();
        player._after = _pendingAfter;
        player._script = _pendingScript;
    }

    private IEnumerator Start()
    {
        // Venimos del tablero con el cursor libre.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        BuildSkipButton();

        _subtitles = ParallelSubtitles.TryStart(_script);
        float elapsed = 0f;
        while (elapsed < MinSeconds || (_subtitles != null && _subtitles.IsPlaying))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(SecondsAfterScript);

        Leave();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)) Leave();
    }

    private void Leave()
    {
        if (_leaving) return;
        _leaving = true;
        StopAllCoroutines();
        if (_subtitles != null) Destroy(_subtitles.gameObject);

        // Al despacho, con pantalla de carga; a cualquier otra escena, directo.
        if (_after == SceneName.Despacho) SceneAdministrator.LoadWithLoadingScreen(_after);
        else SceneAdministrator.Load(_after);
    }

    // Boton semitransparente abajo a la derecha. Se arma por codigo para no tener que
    // ponerlo en cada escena de fragmento. Con el cursor libre (ej: Tab) tambien se clickea.
    private void BuildSkipButton()
    {
        GameObject canvasObject = new GameObject("SkipCanvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 101; // encima de los subtitulos
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject buttonObject = new GameObject("SkipButton", typeof(RectTransform));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-40f, 40f);
        rect.sizeDelta = new Vector2(240f, 64f);

        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.5f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(Leave);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = (RectTransform)labelObject.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = SkipLabel;
        label.fontSize = 28;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 1f, 1f, 0.85f);
        label.raycastTarget = false;
    }
}
