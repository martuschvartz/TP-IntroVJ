namespace _Project.Scripts.Utility
{
    /* Todas las escenas del juego. Usar esto en vez de escribir nombres a mano:
    * en el Inspector aparece como desplegable y no hay typos.
    *
    * OJO: Unity guarda los enums como NÚMERO en las escenas/prefabs.
    * - No cambiar los números de los valores que ya existen.
    * - Las escenas nuevas van al final, con el siguiente número libre.
     */
    public enum SceneName
    {
        #region Pantallas

        Menu = 0,
        Info = 1,
        FinDelDia = 2,
        FinalConfesar = 3,
        FinalNoHacerNada = 4,

        #endregion

        #region Niveles Generales

        Pesadilla = 5,
        Despacho = 6,

        #endregion

        #region Recuerdos

        // Mateo
        Living = 7,
        LivingParalelo = 8,

        // Romina
        Shed = 9,
        
        // Camila
        Car = 10,
        
        #endregion

        
        #region Carga Async
        
        AsyncLoading = 11,

        #endregion
    }

    /* Qué muestra la mira del HUD según lo que el jugador está mirando.
     * Cada IInteractable dice el suyo (propiedad Icon), así PlayerInteractor
     * no necesita saber si es una pista, una puerta o un documento.
     */
    public enum CrosshairIcon
    {
        Dot = 0,    // nada interactuable
        Hand = 1,   // interactuable común (puerta, documento, tablero...)
        Clue = 2,   // pista de un recuerdo -> libreta
    }
}
