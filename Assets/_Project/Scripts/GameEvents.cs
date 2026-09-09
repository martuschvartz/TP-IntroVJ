using System;
using UnityEngine;

// Identifica cada una de las 3 zonas del recorrido. Se pasa como dato en los
// eventos para saber DE QUE zona se habla, sin que los sistemas necesiten
// conocerse entre si.
public enum ZoneId
{
    Living,
    Flashlight,
    Prize
}

// Punto de encuentro entre sistemas que NO se referencian entre si.
// En vez de que RoomManager busque al KeyObject (o GameStateManager al
// TimerController) por referencia directa, cada sistema "anuncia" lo que
// paso aca, y quien este interesado se suscribe a ese anuncio.
//
// Gracias a esto, el sistema de habitaciones y el de timer pueden existir
// y probarse por separado: a ninguno le importa quien esta escuchando.
public static class GameEvents
{
    #region Se encontro el objeto clave de una zona
    #region Se encontro la llave de una zona
    public static event Action<ZoneId> KeyFound;

    public static void RaiseKeyFound(ZoneId zone)
    {
        int suscriptores = KeyFound?.GetInvocationList().Length ?? 0;
        Debug.Log($"[GameEvents] RaiseKeyFound({zone}), suscriptores: {suscriptores}");
        KeyFound?.Invoke(zone);
    }
    #endregion

    #region Se encontro la linterna de una zona
    public static event Action<ZoneId> FlashlightFound;

    public static void RaiseFlashlightFound(ZoneId zone)
    {
        int suscriptores = FlashlightFound?.GetInvocationList().Length ?? 0;
        Debug.Log($"[GameEvents] RaiseFlashlightFound({zone}), suscriptores: {suscriptores}");
        FlashlightFound?.Invoke(zone);
    }
    #endregion

    #region Se encontro el regalo de una zona
    public static event Action<ZoneId> PrizeFound;

    public static void RaisePrizeFound(ZoneId zone)
    {
        int suscriptores = PrizeFound?.GetInvocationList().Length ?? 0;
        Debug.Log($"[GameEvents] RaisePrizeFound({zone}), suscriptores: {suscriptores}");
        PrizeFound?.Invoke(zone);
    }
    #endregion
    #endregion

    #region Se acabo el tiempo
    public static event Action TimeUp;

    public static void RaiseTimeUp()
    {
        TimeUp?.Invoke();
    }
    #endregion

    #region Se completo todo el recorrido de habitaciones
    public static event Action GameWon;

    public static void RaiseGameWon()
    {
        GameWon?.Invoke();
    }
    #endregion
}
