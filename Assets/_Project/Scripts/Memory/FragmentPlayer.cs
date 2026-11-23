using System.Collections;
using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

// Reproduce el fragmento de un sospechoso (la noche vista con sus ojos) cuando el
// tablero se resolvio bien. Las escenas de fragmento (ej: LivingParalelo2) no tienen
// jugador ni logica: solo su camara y el guion en Resources/Guiones/<escena>.txt.
// Este script se crea solo al cargar la escena (no hay que ponerlo a mano), pasa el
// guion como subtitulos y despues sigue a la escena de "despues" (ej: FinMatias).
public class FragmentPlayer : MonoBehaviour
{
    // Si la escena no tiene guion, se queda al menos este tiempo.
    private const float MinSeconds = 5f;
    private const float SecondsAfterScript = 1.5f;

    private static string _pendingScene;
    private static SceneName _pendingAfter;

    private SceneName _after;

    // Lo llama el tablero en vez de cargar la escena directo.
    public static void Play(SceneName fragment, SceneName after)
    {
        _pendingScene = fragment.ToString();
        _pendingAfter = after;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneAdministrator.LoadWithLoadingScreen(fragment);
    }

    // Antes del fragmento se carga AsyncLoading: se espera a que llegue la escena pedida.
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != _pendingScene) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        _pendingScene = null;

        FragmentPlayer player = new GameObject("FragmentPlayer").AddComponent<FragmentPlayer>();
        player._after = _pendingAfter;
    }

    private IEnumerator Start()
    {
        // Venimos del tablero con el cursor libre; en el fragmento solo se mira.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ParallelSubtitles subtitles = ParallelSubtitles.TryStart(gameObject.scene.name);
        float elapsed = 0f;
        while (elapsed < MinSeconds || (subtitles != null && subtitles.IsPlaying))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(SecondsAfterScript);

        // Al despacho, con pantalla de carga; a una reflexion (FinMatias), directo.
        if (_after == SceneName.Despacho) SceneAdministrator.LoadWithLoadingScreen(_after);
        else SceneAdministrator.Load(_after);
    }
}
