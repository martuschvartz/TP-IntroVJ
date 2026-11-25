using System.Collections;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.Managers
{
    /**
     *  Carga SceneAdministrator.NextSceneFile de forma asincronica, mostrando un anillo
     *  que se va completando segun avanza la carga. Cuando se completa, entra sola a la
     *  escena (sin apretar nada).
     */
    public class AsyncLoading : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [Tooltip("Velocidad maxima de llenado del circulo (fraccion por segundo). " +
                 "Evita que el circulo salte de golpe si la escena carga muy rapido.")]
        [SerializeField] private float fillSpeed = 3f;
        [Tooltip("Grosor del anillo, como fraccion del radio.")]
        [Range(0.02f, 1f)] [SerializeField] private float ringThickness = 0.12f;
        [SerializeField] private Color ringColor = Color.white;
        [Tooltip("Opacidad del anillo de fondo (la guia que muestra el circulo completo).")]
        [Range(0f, 1f)] [SerializeField] private float trackAlpha = 0.15f;

        private void Start()
        {
            SetupRing();
            StartCoroutine(LoadScreenAsync());
        }

        // Convierte progressBar en un anillo que se completa en sentido horario desde arriba,
        // con una copia tenue detras como guia.
        private void SetupRing()
        {
            Sprite ring = CreateRingSprite(256, ringThickness);

            progressBar.sprite = ring;
            progressBar.color = ringColor;
            progressBar.type = Image.Type.Filled;
            progressBar.fillMethod = Image.FillMethod.Radial360;
            progressBar.fillOrigin = (int)Image.Origin360.Top;
            progressBar.fillClockwise = true;
            progressBar.fillAmount = 0f;

            Image track = Instantiate(progressBar, progressBar.transform.parent);
            track.name = "RingTrack";
            track.type = Image.Type.Simple;
            track.color = new Color(ringColor.r, ringColor.g, ringColor.b, trackAlpha);
            track.transform.SetSiblingIndex(progressBar.transform.GetSiblingIndex()); // queda detras
        }

        private static Sprite CreateRingSprite(int size, float thickness)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size / 2f;
            float inner = radius * (1f - thickness);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                // borde suavizado de 1 pixel por dentro y por fuera
                float a = Mathf.Clamp01(radius - d) * Mathf.Clamp01(d - inner);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private IEnumerator LoadScreenAsync()
        {
            // Mas tiempo por frame para integrar la escena: carga antes, aunque la animacion
            // de la pantalla de carga vaya un poco menos fluida.
            Application.backgroundLoadingPriority = ThreadPriority.High;

            // La escena se activa sola apenas termina de cargar, sin esperar al anillo.
            // progress va de 0 a 0.9 mientras lee la escena y de 0.9 a 1 mientras la activa
            // (Awake/Start de todos los objetos). Esa ultima parte bloquea el juego, asi que el
            // anillo queda quieto cerca del 90% hasta que aparece la escena.
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(SceneAdministrator.NextSceneFile);
            float shown = 0f;

            while (!loadingOperation.isDone)
            {
                shown = Mathf.MoveTowards(shown, loadingOperation.progress, fillSpeed * Time.unscaledDeltaTime);
                progressBar.fillAmount = shown;
                yield return null;
            }
        }

        private void OnDestroy()
        {
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
        }
    }
}