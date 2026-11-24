using System.Collections;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.Managers
{
    /**
     *  Sería una clase que carga _targetScreenToLoad de forma asincrónica
     */
    public class AsyncLoading : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private Text progressValue;
        
        private void Start()
        {
            StartCoroutine(LoadScreenAsync());
        }

        IEnumerator LoadScreenAsync() {
            string targetScene = SceneAdministrator.NextSceneFile;
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(targetScene);
            loadingOperation.allowSceneActivation = false;
            float progress;
    
            while (!loadingOperation.isDone) {
                progress = loadingOperation.progress;
                progressBar.fillAmount = progress;
                progressValue.text = $"Loading... {progress * 100}%";
                if (progress >= 0.9f) {
                    progressValue.text = "Press SPACE to continue";
                    if (Input.GetKeyDown(KeyCode.Space)) loadingOperation.allowSceneActivation = true;
                }
                yield return null;
            }
        }
    }
}