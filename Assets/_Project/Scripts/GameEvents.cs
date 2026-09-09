using System;
using UnityEngine;

// Identifica cada una de las 3 zonas del recorrido. Se pasa como dato en los
// eventos para saber DE QUE zona se habla, sin que los sistemas necesiten
// conocerse entre si.
public enum ZoneId
{
    Living,
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
    public static event Action<ZoneId> ObjectFound;

    public static void RaiseObjectFound(ZoneId zone)
    {
        // debug: sacar este log despues. Si "suscriptores" da 0, el evento
        // se publico bien pero nadie lo esta escuchando (RoomManager
        // deshabilitado, no esta en la escena, o su OnEnable no se disparo).
        int suscriptores = ObjectFound?.GetInvocationList().Length ?? 0;
        Debug.Log($"[GameEvents] RaiseObjectFound({zone}), suscriptores: {suscriptores}");

        // El "?." evita un error si todavia no hay nadie suscripto
        // (por ejemplo, si esto se llama antes de que RoomManager exista
        // en la escena, o en una escena de prueba sin RoomManager).
        ObjectFound?.Invoke(zone);
    }
    #endregion
}
