using System;

// Punto de encuentro entre sistemas que NO se referencian entre si.
// Cada sistema "anuncia" lo que paso aca, y quien este interesado se
// suscribe a ese anuncio. Asi, por ejemplo, la pista no necesita saber
// que existe una lista en pantalla que la tiene que tildar.
public static class GameEvents
{
    #region Se encontro una pista en un recuerdo
    // Lo lanza MemoryClue. Lo escuchan MemoryManager (lleva la cuenta)
    // y ClueListUI (tilda el renglon).
    public static event Action<MemoryClue> ClueFound;

    public static void RaiseClueFound(MemoryClue clue)
    {
        ClueFound?.Invoke(clue);
    }
    #endregion

    #region Se completo un recuerdo (las 3 pistas encontradas)
    // Lo lanza MemoryManager. Lo escucha ClueListUI (muestra la lista
    // completa mientras se sale del recuerdo).
    public static event Action<string> MemoryCompleted;

    public static void RaiseMemoryCompleted(string memoryId)
    {
        MemoryCompleted?.Invoke(memoryId);
    }
    #endregion

    #region Se bloquea / desbloquea al jugador
    // Lo lanzan la cinematica del despacho, el panel de decision y el
    // MemoryManager al salir. Lo escucha FirstPersonPlayer.
    public static event Action<bool> PlayerLockChanged;

    public static void RaisePlayerLockChanged(bool locked)
    {
        PlayerLockChanged?.Invoke(locked);
    }
    #endregion
}
