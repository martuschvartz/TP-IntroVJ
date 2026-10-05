using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Te lleva un rato al cuarto paralelo, que es otra escena, y te devuelve.
// La escena se carga en modo ADITIVO: se suma encima del recuerdo en vez de
// reemplazarlo. Asi el recuerdo (pistas encontradas, lista, cola) sigue vivo.
public class VisitParallelSceneCommand : ICommand
{
    private readonly string _sceneName;
    private readonly Transform _player;
    private readonly float _seconds;
    private readonly GameObject _hideWhileAway;

    public VisitParallelSceneCommand(string sceneName, Transform player, float seconds, GameObject hideWhileAway)
    {
        _sceneName = sceneName;
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
        yield return SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        Scene parallelScene = SceneManager.GetSceneByName(_sceneName);

        // La escena activa es la que pone la iluminacion (skybox, luz ambiente).
        SceneManager.SetActiveScene(parallelScene);
        if (_hideWhileAway != null) _hideWhileAway.SetActive(false);

        // 2. Ir al cuarto paralelo. Si la escena no tiene ParallelRoomPoint,
        // el jugador se queda en las mismas coordenadas.
        ParallelRoomPoint point = Object.FindAnyObjectByType<ParallelRoomPoint>();
        if (point != null)
        {
            yield return new TeleportCommand(_player, point.transform.position, point.transform.rotation).Execute();
        }

        // 3. Quedarse un rato.
        yield return new WaitForSeconds(_seconds);

        // 4. Volver a donde estabas y descargar el cuarto paralelo.
        if (_hideWhileAway != null) _hideWhileAway.SetActive(true);
        if (point != null)
        {
            yield return new TeleportCommand(_player, returnPosition, returnRotation).Execute();
        }
        SceneManager.SetActiveScene(memoryScene);
        yield return SceneManager.UnloadSceneAsync(parallelScene);
    }
}
