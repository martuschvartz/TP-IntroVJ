using System.Collections;
using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using UnityEngine;

// Manager de un recuerdo (hay uno por escena de recuerdo).
// Lleva la cuenta de las pistas encontradas, decide cuando se puede activar
// el trigger y cuando se sale del recuerdo.
public class MemoryManager : MonoBehaviour
{
    [Tooltip("Id del recuerdo. Tiene que coincidir con el del MemoryDocument del despacho. Ej: Mateo")]
    [SerializeField] private string memoryId;
    [Tooltip("El sospechoso de este recuerdo (sus declaraciones se muestran en la lista del Tab).")]
    [SerializeField] private SuspectData suspect;
    [SerializeField] private MemoryClue[] clues;
    [SerializeField] private CommandQueue queue;
    [Tooltip("Segundos que se queda en el recuerdo después de encontrar todo, antes de salir.")]
    [SerializeField] private float exitDelay = 2f;
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

        if (AllFound()) StartCoroutine(ExitWhenReady());
    }

    private IEnumerator ExitWhenReady()
    {
        GameEvents.RaiseMemoryCompleted(memoryId);

        // Si el ultimo fue el trigger, primero termina su evento (la cola).
        while (queue.IsBusy) yield return null;

        yield return new WaitForSeconds(exitDelay);
        Exit();
    }

    // Unica salida del recuerdo: la usan la salida automatica (todo encontrado) y el
    // boton de volver antes de la lista del Tab. El dia que haya timer, tambien llama a esto.
    public void Exit()
    {
        // Por si se aprieta el boton justo mientras corre la salida automatica.
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
