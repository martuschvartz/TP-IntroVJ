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
        CarParallel = 15,
        
        #endregion

        
        #region Carga Async
        
        AsyncLoading = 11,

        #endregion

        #region Fragmentos y reflexiones

        // Mateo: el fragmento (01:10-01:30 en el living) y la reflexion de despues.
        LivingParalelo2 = 12,
        FinMatias = 13,

        // Mateo: el living armado con los personajes, donde se ve el fragmento
        // al cerrar su caso desde el tablero. El archivo se llama "Living-template".
        LivingTemplate = 14,

        // Romina: el cobertizo armado con los personajes, donde se ve el fragmento
        // al cerrar su caso con la contradiccion pisadas <-> sendero. El archivo se llama "shedParalel2".
        ShedParalel2 = 16,

        #endregion
    }

    public static class SceneNameExtensions
    {
        // Nombre del archivo de la escena. Casi siempre es igual al del enum;
        // los que no pueden serlo (ej: tienen un guion) se mapean aca.
        public static string FileName(this SceneName scene)
        {
            switch (scene)
            {
                case SceneName.LivingTemplate: return "Living-template";
                case SceneName.ShedParalel2: return "shedParalel2";
                default: return scene.ToString();
            }
        }
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
