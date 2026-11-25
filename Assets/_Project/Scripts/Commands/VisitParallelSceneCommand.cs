using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

// Te lleva un rato al cuarto paralelo, que es otra escena, y te devuelve.
// La escena se carga en modo ADITIVO: se suma encima del recuerdo en vez de
// reemplazarlo. Asi el recuerdo (pistas encontradas, lista, cola) sigue vivo.
//
// Mientras estas alla, todo lo del recuerdo original se oculta (se ve, se
// choca e ilumina solo el cuarto paralelo). No se apagan los GameObjects
// enteros sino sus Renderers, Colliders y Lights: si apagaramos el objeto que
// tiene la CommandQueue, se cortaria esta misma corrutina.
public class VisitParallelSceneCommand : ICommand
{
    private readonly SceneName _scene;
    private readonly Transform _player;
    private readonly float _seconds;
    private readonly GameObject _hideWhileAway;

    // Lo que apagamos al irnos, para volver a prender exactamente eso al volver.
    private readonly List<Renderer> _hiddenRenderers = new List<Renderer>();
    private readonly List<Collider> _hiddenColliders = new List<Collider>();
    private readonly List<Light> _hiddenLights = new List<Light>();

    public VisitParallelSceneCommand(SceneName scene, Transform player, float seconds, GameObject hideWhileAway)
    {
        _scene = scene;
        _player = player;
        _seconds = seconds;
        _hideWhileAway = hideWhileAway;
    }

    public IEnumerator Execute()
    {
        Vector3 returnPosition = _player.position;
        Quaternion returnRotation = _player.rotation;
        Scene memoryScene = SceneManager.GetActiveScene();

        // 1. Cargar el cuarto paralelo encima del recuerdo y esperar a que termine.
        string sceneFile = _scene.FileName();
        yield return SceneManager.LoadSceneAsync(sceneFile, LoadSceneMode.Additive);
        Scene parallelScene = SceneManager.GetSceneByName(sceneFile);

        // La escena activa es la que pone la iluminacion (skybox, luz ambiente).
        SceneManager.SetActiveScene(parallelScene);
        HideMemoryScene(memoryScene);
        if (_hideWhileAway != null) _hideWhileAway.SetActive(false);

        // 2. Ir al cuarto paralelo. Si la escena no tiene ParallelRoomPoint,
        // el jugador se queda en las mismas coordenadas.
        ParallelRoomPoint point = Object.FindAnyObjectByType<ParallelRoomPoint>();
        if (point != null)
        {
            yield return new TeleportCommand(_player, point.transform.position, point.transform.rotation).Execute();
        }

        // 3. Quedarse un rato. Si la escena tiene guion (Resources/Guiones/<escena>.txt),
        // se muestra como subtitulos y no se vuelve hasta que termine de pasar.
        ParallelSubtitles subtitles = ParallelSubtitles.TryStart(sceneFile);
        float elapsed = 0f;
        while (elapsed < _seconds || (subtitles != null && subtitles.IsPlaying))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (subtitles != null) Object.Destroy(subtitles.gameObject);

        // 4. Volver a donde estabas y descargar el cuarto paralelo.
        if (_hideWhileAway != null) _hideWhileAway.SetActive(true);
        ShowMemoryScene();
        if (point != null)
        {
            yield return new TeleportCommand(_player, returnPosition, returnRotation).Execute();
        }
        SceneManager.SetActiveScene(memoryScene);
        yield return SceneManager.UnloadSceneAsync(parallelScene);
    }

    // Apaga lo que se ve, se choca o ilumina en la escena del recuerdo, menos el jugador.
    private void HideMemoryScene(Scene memoryScene)
    {
        foreach (GameObject root in memoryScene.GetRootGameObjects())
        {
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            {
                if (r.enabled && !IsPlayer(r.transform)) { r.enabled = false; _hiddenRenderers.Add(r); }
            }
            foreach (Collider c in root.GetComponentsInChildren<Collider>())
            {
                if (c.enabled && !IsPlayer(c.transform)) { c.enabled = false; _hiddenColliders.Add(c); }
            }
            foreach (Light l in root.GetComponentsInChildren<Light>())
            {
                if (l.enabled && !IsPlayer(l.transform)) { l.enabled = false; _hiddenLights.Add(l); }
            }
        }
    }

    private void ShowMemoryScene()
    {
        foreach (Renderer r in _hiddenRenderers) if (r != null) r.enabled = true;
        foreach (Collider c in _hiddenColliders) if (c != null) c.enabled = true;
        foreach (Light l in _hiddenLights) if (l != null) l.enabled = true;
        _hiddenRenderers.Clear();
        _hiddenColliders.Clear();
        _hiddenLights.Clear();
    }

    private bool IsPlayer(Transform t) => t.IsChildOf(_player);
}
