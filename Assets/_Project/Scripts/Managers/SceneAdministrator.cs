using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Managers
{
    public class SceneAdministrator : MonoBehaviour
    {
        // Pantallas
        private const string MENU = "Menu";
        private const string INFO = "Info";
        
        // Niveles
        private const string DESPACHO = "Despacho";

        public void LoadMenu() => SceneManager.LoadScene(MENU);
        public void LoadInfo() => SceneManager.LoadScene(INFO);
        public void LoadDespacho() => SceneManager.LoadScene(DESPACHO);
        public void QuitGame() => Application.Quit();
    }
}