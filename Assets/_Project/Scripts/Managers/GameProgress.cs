using System.Collections.Generic;

// Lo que el juego recuerda a lo largo de la partida, entre escenas.
// Es una clase estatica (no un MonoBehaviour): no vive en ninguna escena,
// asi que no se pierde al cambiar de escena.
public static class GameProgress
{
    public const int TotalMemories = 3;

    private static readonly HashSet<string> VisitedMemories = new HashSet<string>();

    // Para que la conversacion del despacho pase una sola vez.
    public static bool OfficeCinematicSeen { get; set; }

    public static bool AllVisited => VisitedMemories.Count >= TotalMemories;

    public static void MarkVisited(string memoryId) => VisitedMemories.Add(memoryId);

    public static bool IsVisited(string memoryId) => VisitedMemories.Contains(memoryId);

    public static void Reset()
    {
        VisitedMemories.Clear();
        OfficeCinematicSeen = false;
    }
}
