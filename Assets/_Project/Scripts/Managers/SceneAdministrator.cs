using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Managers
{
    public class SceneAdministrator : MonoBehaviour
    {
        // Pantallas
        public const string MENU = "Menu";
        public const string INFO = "Info";
        public const string FIN_DEL_DIA = "FinDelDia";
        public const string FINAL_CONFESAR = "FinalConfesar";
        public const string FINAL_NO_HACER_NADA = "FinalNoHacerNada";

        // Niveles
        public const string PESADILLA = "Pesadilla";
        public const string DESPACHO = "Despacho";

        public void LoadMenu() => SceneManager.LoadSceneAsync(MENU);
        public void LoadInfo() => SceneManager.LoadSceneAsync(INFO);
        public void LoadDespacho() => SceneManager.LoadSceneAsync(DESPACHO);
        public void LoadFinalConfesar() => SceneManager.LoadSceneAsync(FINAL_CONFESAR);
        public void LoadFinalNoHacerNada() => SceneManager.LoadSceneAsync(FINAL_NO_HACER_NADA);
        public void QuitGame() => Application.Quit();

        // Para el botón "Jugar" del menú: arranca una partida de cero.
        public void StartNewGame()
        {
            GameProgress.Reset();
            SceneManager.LoadSceneAsync(PESADILLA);
        }
    }
}