using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Managers
{
    public class SceneAdministrator : MonoBehaviour
    {
        // Archivo de la escena que va a cargar la pantalla de carga (AsyncLoading).
        public static string NextSceneFile { get; private set; } = SceneName.Despacho.FileName();

        private static void Load(string scene) => SceneManager.LoadSceneAsync(scene);

        // Carga directa, sin pantalla de carga. Para usar desde código.
        public static void Load(SceneName scene) => Load(scene.FileName());

        public static void LoadWithLoadingScreen(SceneName scene) => LoadWithLoadingScreen(scene.FileName());

        // Igual, pero con el nombre del archivo de la escena (para escenas que no estan en SceneName,
        // como el endOfDayScene del MemoryManager). Tiene que estar en Build Profiles.
        public static void LoadWithLoadingScreen(string sceneFile)
        {
            NextSceneFile = sceneFile;
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