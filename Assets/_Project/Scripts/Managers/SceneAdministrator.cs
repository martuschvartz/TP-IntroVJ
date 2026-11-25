using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Managers
{
    public class SceneAdministrator : MonoBehaviour
    {
        public static SceneName NextScene { get; private set; } = SceneName.Despacho;

        // Carga directa, sin pantalla de carga.
        public static void Load(SceneName scene) => SceneManager.LoadSceneAsync(scene.FileName());

        public static void LoadWithLoadingScreen(SceneName scene)
        {
            NextScene = scene;
            Load(SceneName.AsyncLoading);
        }

        public void LoadNewGame()
        {
            GameProgress.Reset();
            // Arranca por la pesadilla (escena casi vacia, sin pantalla de carga);
            // al terminar, ella pasa al despacho.
            Load(SceneName.Pesadilla);
        }
        public void LoadMenu() => Load(SceneName.Menu);
        public void LoadInfo() => Load(SceneName.Info);
        public void LoadFinalConfesar() => Load(SceneName.Carcel);
        public void LoadFinalNoHacerNada() => Load(SceneName.NoCarcel);
        public void QuitGame() => Application.Quit();
    }
}
