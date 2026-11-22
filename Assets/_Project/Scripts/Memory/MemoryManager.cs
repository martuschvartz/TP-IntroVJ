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

    public SuspectData Suspect => suspect;
    public MemoryClue[] Clues => clues;

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

    // Unica salida del recuerdo. El dia que haya timer, tambien llama a esto.
    public void Exit()
    {
        GameEvents.RaisePlayerLockChanged(true);
        GameProgress.MarkVisited(memoryId);
        // El fin del dia es el tablero del despacho en modo edicion.
        GameProgress.EndOfDayPending = true;
        SceneAdministrator.LoadWithLoadingScreen(SceneName.Despacho);
    }

    private bool AllFound()
    {
        foreach (MemoryClue clue in clues)
        {
            if (!clue.Found) return false;
        }
        return true;
    }

    private void SetTriggersLocked(bool locked)
    {
        foreach (MemoryClue clue in clues)
        {
            if (clue.Type == ClueType.Trigger) clue.SetLocked(locked);
        }
    }
}
