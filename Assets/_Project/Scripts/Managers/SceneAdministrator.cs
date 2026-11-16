using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Managers
{
    public class SceneAdministrator : MonoBehaviour
    {
        public static SceneName NextScene { get; private set; } = SceneName.Despacho;

        private static void Load(string scene) => SceneManager.LoadSceneAsync(scene);

        // Carga directa, sin pantalla de carga. Para usar desde código.
        public static void Load(SceneName scene) => Load(scene.ToString());

        public static void LoadWithLoadingScreen(SceneName scene)
        {
            NextScene = scene;
            Load(SceneName.AsyncLoading.ToString());
        }
        
        public void LoadNewGame()
        {
            GameProgress.Reset();                          
            LoadWithLoadingScreen(SceneName.Despacho);     
        }
        public void LoadMenu() => Load(SceneName.Menu.ToString());
        public void LoadInfo() => Load(SceneName.Info.ToString());
        public void LoadFinalConfesar() => Load(SceneName.FinalConfesar.ToString());
        public void LoadFinalNoHacerNada() => Load(SceneName.FinalNoHacerNada.ToString());
        public void QuitGame() => Application.Quit();
    }
}