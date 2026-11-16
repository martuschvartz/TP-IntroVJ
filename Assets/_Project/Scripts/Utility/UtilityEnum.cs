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
}
