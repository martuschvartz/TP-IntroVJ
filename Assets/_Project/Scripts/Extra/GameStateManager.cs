using UnityEngine;

// Coordina el estado general de la partida. Es el unico script que conoce
// tanto al timer como al RoomManager, porque su trabajo es justamente
// conectar "se acabo el tiempo" / "se completo el recorrido" con que pantalla
// mostrar. Ni el timer ni las habitaciones saben que este script existe.
public class GameStateManager : MonoBehaviour
{
    private enum State { Playing, Won, Lost }

    [SerializeField] private RoomManager _roomManager;
    [SerializeField] private TimerController _timer;
    [SerializeField] private GameObject _winScreen;      // cartel "You Win", inactivo al arrancar
    [SerializeField] private GameObject _gameOverScreen; // cartel de Game Over, inactivo al arrancar

    private State _state = State.Playing;

    #region Suscripcion a eventos
    private void OnEnable()
    {
        GameEvents.GameWon += HandleGameWon;
        GameEvents.TimeUp += HandleTimeUp;
    }

    private void OnDisable()
    {
        GameEvents.GameWon -= HandleGameWon;
        GameEvents.TimeUp -= HandleTimeUp;
    }
    #endregion

    private void HandleGameWon()
    {
        if (_state != State.Playing) return;

        // A diferencia de perder, ganar no oculta las habitaciones: el
        // jugador se queda viendo el mundo, solo aparece el cartel.
        _state = State.Won;
        _timer.Pause();
        ShowScreen(_winScreen);
    }

    private void HandleTimeUp()
    {
        // Si ya ganaste, el timer deberia estar pausado y esto no deberia
        // llegar a pasar. El chequeo queda igual por si los eventos llegan
        // en un orden inesperado.
        if (_state != State.Playing) return;

        _state = State.Lost;
        _roomManager.HideAll();
        ShowScreen(_gameOverScreen);
    }

    private void ShowScreen(GameObject screen)
    {
        if (screen != null)
        {
            screen.SetActive(true);
        }
    }
}
