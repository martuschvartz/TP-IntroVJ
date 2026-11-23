using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using UnityEngine;

// Manager de un recuerdo (hay uno por escena de recuerdo).
// Lleva la cuenta de las pistas encontradas, decide cuando se puede activar
// el trigger y cuando se puede salir del recuerdo (siempre con el boton de la lista del Tab).
public class MemoryManager : MonoBehaviour
{
    [Tooltip("Id del recuerdo. Tiene que coincidir con el del MemoryDocument del despacho. Ej: Mateo")]
    [SerializeField] private string memoryId;
    [Tooltip("El sospechoso de este recuerdo (sus declaraciones se muestran en la lista del Tab).")]
    [SerializeField] private SuspectData suspect;
    [SerializeField] private MemoryClue[] clues;
    [Tooltip("Con esta cantidad de pistas encontradas se puede volver al despacho antes (botón en la lista del Tab).")]
    [SerializeField, Min(1)] private int minCluesToExit = 3;

    private bool _exiting;

    public SuspectData Suspect => suspect;
    public MemoryClue[] Clues => clues;
    public int TotalClues => clues.Length;
    public bool AllCluesFound => AllFound();

    // Se puede salir antes con el minimo (o con todas, si el recuerdo tiene menos que el minimo).
    public bool CanExitEarly => !_exiting && FoundCount >= Mathf.Min(minCluesToExit, clues.Length);

    public int FoundCount
    {
        get
        {
            int found = 0;
            foreach (MemoryClue clue in clues)
            {
                if (clue.Found) found++;
            }
            return found;
        }
    }

    #region Suscripcion a eventos
    private void OnEnable()
    {
        GameEvents.ClueFound += HandleClueFound;
    }

    private void OnDisable()
    {
        GameEvents.ClueFound -= HandleClueFound;
    }
    #endregion

    private void Start()
    {
        // El trigger arranca bloqueado: primero hay que detectar la mentira.
        SetTriggersLocked(true);
    }

    private void HandleClueFound(MemoryClue clue)
    {
        // Queda guardada para el tablero del despacho.
        GameProgress.AddFoundClue(clue.Data);

        if (clue.Type == ClueType.Inconsistencia) SetTriggersLocked(false);

        // Con todo encontrado no se sale solo: se avisa (la lista se abre) y el jugador
        // vuelve al despacho con el boton cuando quiera.
        if (AllFound()) GameEvents.RaiseMemoryCompleted(memoryId);
    }

    // Unica salida del recuerdo: la usa el boton "Volver al despacho" de la lista del Tab.
    // El dia que haya timer, tambien llama a esto.
    public void Exit()
    {
        // Por si se aprieta el boton dos veces antes del cambio de escena.
        if (_exiting) return;
        _exiting = true;

        GameEvents.RaisePlayerLockChanged(true);
        GameProgress.MarkVisited(memoryId);
        // El fin del dia es el tablero del despacho en modo edicion.
        GameProgress.EndOfDayPending = true;
        SceneAdministrator.LoadWithLoadingScreen(SceneName.Despacho);
    }

    private bool AllFound() => FoundCount == clues.Length;

    private void SetTriggersLocked(bool locked)
    {
        foreach (MemoryClue clue in clues)
        {
            if (clue.Type == ClueType.Trigger) clue.SetLocked(locked);
        }
    }
}
