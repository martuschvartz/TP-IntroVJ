using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Atajo F5: reinicia la partida en cualquier momento, sin salir del Play mode.
// Hace lo mismo que "Jugar" en el menu: borra el progreso y carga el despacho.
//
// No hay que ponerlo en ninguna escena: se crea solo al arrancar el juego
// y sobrevive a los cambios de escena. En el menu, Info y la pantalla de carga no hace nada.
public class RestartShortcut : MonoBehaviour
{
    private const Key Shortcut = Key.F5;

    private static RestartShortcut _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (_instance != null) return;
        var go = new GameObject("[RestartShortcut]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<RestartShortcut>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || !keyboard[Shortcut].wasPressedThisFrame) return;

        string scene = SceneManager.GetActiveScene().name;
        if (scene == SceneName.Menu.FileName()
            || scene == SceneName.Info.FileName()
            || scene == SceneName.AsyncLoading.FileName()) return;

        Time.timeScale = 1f;
        GameProgress.Reset();
        SceneAdministrator.LoadWithLoadingScreen(SceneName.Despacho);
    }
}
