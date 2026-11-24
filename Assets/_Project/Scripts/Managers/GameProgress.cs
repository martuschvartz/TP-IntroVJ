using System.Collections.Generic;
using UnityEngine;

// Lo que el juego recuerda a lo largo de la partida, entre escenas.
// Es una clase estatica (no un MonoBehaviour): no vive en ninguna escena,
// asi que no se pierde al cambiar de escena.
public static class GameProgress
{
    public const int TotalMemories = 3;

    private static readonly HashSet<string> VisitedMemories = new HashSet<string>();
    // Sospechosos cerrados desde el tablero ("Cerrar sospechoso"): su documento ya no aparece.
    private static readonly HashSet<string> ClosedSuspects = new HashSet<string>();

    // Tablero: pistas encontradas y los hilos que tiro el jugador.
    // Se guardan como assets (ClueData / StatementData) porque los MemoryClue
    // se destruyen al salir del recuerdo.
    private static readonly List<ClueData> FoundCluesList = new List<ClueData>();
    private static readonly List<Connection> ConnectionsList = new List<Connection>();
    // Donde dejo el jugador cada tarjeta (pista o declaracion), para que el tablero
    // se vea igual cuando lo vuelve a mirar.
    private static readonly Dictionary<ScriptableObject, Vector2> CardPositions = new Dictionary<ScriptableObject, Vector2>();

    // Para que la conversacion del despacho pase una sola vez.
    public static bool OfficeCinematicSeen { get; set; }

    // Se prende al salir de un recuerdo: el despacho abre el tablero en modo edicion
    // (fin del dia). Lo apaga el tablero al "Finalizar investigacion por hoy".
    public static bool EndOfDayPending { get; set; }

    public static bool AllVisited => VisitedMemories.Count >= TotalMemories;

    // El ultimo recuerdo del que se salio: es el que se valida al cerrar el dia.
    public static string LastVisitedMemory { get; private set; }

    public static IReadOnlyList<ClueData> FoundClues => FoundCluesList;
    public static IReadOnlyList<Connection> Connections => ConnectionsList;

    public static void MarkVisited(string memoryId)
    {
        VisitedMemories.Add(memoryId);
        LastVisitedMemory = memoryId;
    }

    public static bool IsVisited(string memoryId) => VisitedMemories.Contains(memoryId);

    public static void CloseSuspect(string suspectId) => ClosedSuspects.Add(suspectId);

    public static bool IsClosed(string suspectId) => ClosedSuspects.Contains(suspectId);

    #region Tablero

    public static bool IsClueFound(ClueData clue) => clue != null && FoundCluesList.Contains(clue);

    public static void AddFoundClue(ClueData clue)
    {
        if (clue == null || FoundCluesList.Contains(clue)) return;
        FoundCluesList.Add(clue);
    }

    // Entre una pista y una declaracion hay a lo sumo un hilo: si ya habia uno, se reemplaza.
    public static void SetConnection(ClueData clue, StatementData statement, ThreadColor color)
    {
        RemoveConnection(clue, statement);
        ConnectionsList.Add(new Connection(clue, statement, color));
    }

    public static void RemoveConnection(ClueData clue, StatementData statement)
    {
        ConnectionsList.RemoveAll(c => c.Joins(clue, statement));
    }

    public static Connection FindConnection(ClueData clue, StatementData statement)
    {
        return ConnectionsList.Find(c => c.Joins(clue, statement));
    }

    public static bool TryGetCardPosition(ScriptableObject card, out Vector2 position)
    {
        return CardPositions.TryGetValue(card, out position);
    }

    public static void SetCardPosition(ScriptableObject card, Vector2 position)
    {
        CardPositions[card] = position;
    }

    #endregion

    public static void Reset()
    {
        VisitedMemories.Clear();
        ClosedSuspects.Clear();
        FoundCluesList.Clear();
        ConnectionsList.Clear();
        CardPositions.Clear();
        LastVisitedMemory = null;
        OfficeCinematicSeen = false;
        EndOfDayPending = false;
    }
}
