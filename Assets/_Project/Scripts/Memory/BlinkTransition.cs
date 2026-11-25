using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// "Parpadeo" antes de entrar a un fragmento: la pantalla se pone negra y vuelve
// dos veces en milisegundos, como un mareo, y recien ahi se cambia de escena.
// Lo usa FragmentPlayer.Play. Se arma solo por codigo (no va en ninguna escena):
// crea un canvas negro por encima de todo, corre el parpadeo, llama a onDone
// (que carga la escena) y se queda en negro hasta que entra la escena nueva.
public class BlinkTransition : MonoBehaviour
{
    // Tiempos en segundos. Cada parpadeo cierra y abre con un fade corto (como un parpado),
    // asi se lee como algo a proposito y no como un glitch.
    private const float Close = 0.08f;       // fade a negro de cada parpadeo
    private const float Open = 0.12f;        // fade de vuelta
    private const float FirstBlack = 0.22f;  // cuanto queda negro el primer parpadeo
    private const float FirstGap = 0.35f;    // se ve la escena entre parpadeos
    private const float SecondBlack = 0.3f;  // el segundo, un poco mas largo
    private const float SecondGap = 0.45f;
    private const float FinalFade = 0.5f;    // el corte final a negro

    private static BlinkTransition _running;

    private Image _black;
    private Action _onDone;

    public static void Run(Action onDone)
    {
        if (_running != null) return; // ya hay un parpadeo en curso: no cargar dos veces

        var go = new GameObject("[BlinkTransition]");
        DontDestroyOnLoad(go);
        _running = go.AddComponent<BlinkTransition>();
        _running._onDone = onDone;
    }

    private void Awake()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // encima de toda la UI (tablero incluido)

        var imageObject = new GameObject("Black", typeof(RectTransform));
        var rt = (RectTransform)imageObject.transform;
        rt.SetParent(transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _black = imageObject.AddComponent<Image>();
        _black.raycastTarget = true; // que no se pueda clickear nada mientras parpadea
        SetAlpha(0f);
    }

    private IEnumerator Start()
    {
        // Tiempo real: el parpadeo anda igual aunque el juego este en pausa.
        yield return Fade(0f, 1f, Close);
        yield return new WaitForSecondsRealtime(FirstBlack);
        yield return Fade(1f, 0f, Open);
        yield return new WaitForSecondsRealtime(FirstGap);

        yield return Fade(0f, 1f, Close);
        yield return new WaitForSecondsRealtime(SecondBlack);
        yield return Fade(1f, 0f, Open);
        yield return new WaitForSecondsRealtime(SecondGap);

        // Corte final a negro, mas lento, y se carga la escena.
        yield return Fade(0f, 1f, FinalFade);

        // Se queda negro hasta que cambia la escena, asi no se ve el tablero un frame mas.
        SceneManager.sceneLoaded += OnSceneLoaded;
        _onDone?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Destroy(gameObject);

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_running == this) _running = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            SetAlpha(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration)));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a) => _black.color = new Color(0f, 0f, 0f, a);
}
