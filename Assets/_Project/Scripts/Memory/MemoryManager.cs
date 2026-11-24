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
    [Tooltip("Escena de reflexion de 'fin del dia' de ESTE recuerdo (ej: FinMatias). Tiene que estar en Build Profiles. "
             + "Despues de esa escena se vuelve al despacho con el tablero. Vacio = directo al tablero.")]
    [SerializeField] private string endOfDayScene;

    private bool _exiting;

    public SuspectData Suspect => suspect;
    public MemoryClue[] Clues => clues;
    // Los huecos de la lista (pista borrada de la escena) no cuentan: si no, el Tab tira error y no aparece "Volver al despacho".
    public int TotalClues => System.Array.FindAll(clues, c => c != null).Length;
    public bool AllCluesFound => AllFound();

    // Se puede salir antes con el minimo (o con todas, si el recuerdo tiene menos que el minimo).
    public bool CanExitEarly => !_exiting && FoundCount >= Mathf.Min(minCluesToExit, TotalClues);

    public int FoundCount
    {
        get
        {
            int found = 0;
            foreach (MemoryClue clue in clues)
            {
                if (clue != null && clue.Found) found++;
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

    // Si ya se habia entrado a este recuerdo (el caso sigue abierto), las pistas que se
    // encontraron la otra vez quedan encontradas. Va en Awake para que la lista del Tab
    // ya las vea en su Start.
    private void Awake()
    {
        foreach (MemoryClue clue in clues)
        {
            if (clue != null && GameProgress.IsClueFound(clue.Data)) clue.RestoreFound();
        }
    }

    private void Start()
    {
        // El trigger arranca bloqueado: primero hay que detectar la mentira
        // (salvo que ya se haya detectado en una visita anterior).
        SetTriggersLocked(!InconsistencyFound());
    }

    private bool InconsistencyFound()
    {
        foreach (MemoryClue clue in clues)
        {
            if (clue != null && clue.Found && clue.Type == ClueType.Inconsistencia) return true;
        }
        return false;
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

        // Si el recuerdo tiene su escena de reflexion, se pasa primero por ahi
        // (EndOfDaySequence vuelve despues al despacho).
        if (!string.IsNullOrWhiteSpace(endOfDayScene)) SceneAdministrator.LoadWithLoadingScreen(endOfDayScene);
        else SceneAdministrator.LoadWithLoadingScreen(SceneName.Despacho);
    }

    private bool AllFound() => FoundCount == TotalClues;

    private void SetTriggersLocked(bool locked)
    {
        foreach (MemoryClue clue in clues)
        {
            if (clue != null && clue.Type == ClueType.Trigger) clue.SetLocked(locked);
        }
    }
}
