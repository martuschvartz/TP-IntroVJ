using System.Collections;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.Managers
{
    /**
     *  Carga SceneAdministrator.NextScene de forma asincronica, mostrando el garabato
     *  (progressBar, Filled Radial360 configurado en la escena) que se va dibujando segun
     *  avanza la carga. Cuando termina, entra sola a la escena (sin apretar nada).
     */
    public class AsyncLoading : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private float fillSpeed = 3f;
        [SerializeField, Min(0f)] private float minDuration = 0.6f;

        private void Start()
        {
            progressBar.fillAmount = 0f;
            StartCoroutine(LoadScreenAsync());
        }

        private IEnumerator LoadScreenAsync()
        {
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(SceneAdministrator.NextScene.FileName());
            loadingOperation.allowSceneActivation = false;
            float startTime = Time.unscaledTime;
            float shown = 0f;
            
            while (loadingOperation.progress < 0.9f || shown < 1f || Time.unscaledTime - startTime < minDuration)
            {
                float target = Mathf.Clamp01(loadingOperation.progress / 0.9f);
                shown = Mathf.MoveTowards(shown, target, fillSpeed * Time.unscaledDeltaTime);
                progressBar.fillAmount = shown;
                yield return null;
            }
            
            loadingOperation.allowSceneActivation = true;
        }
    }
}
