using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
// Poné este script en un GameObject vacío dentro de la escena "room".
// Cuenta hacia atrás, muestra el tiempo en un texto de UI (opcional)
// y al llegar a cero funde la pantalla y vuelve a "Despacho".
public class TimerController : MonoBehaviour
{
    [Header("Tiempo")]
    [Tooltip("Duración del recuerdo en segundos")]
    [SerializeField] private float duration = 60f;

    [Header("UI (opcional)")]
    [Tooltip("Texto TextMeshPro donde se muestra el tiempo. Puede quedar vacío.")]
    [SerializeField] private TMP_Text timerText;

    [Header("Al terminar")]
    [SerializeField] private string returnScene = "Despacho";
    [SerializeField] private Color fadeColor = Color.white;
    [SerializeField] private float fadeDuration = 1.2f;

    [Tooltip("Cosas extra que quieras que pasen al terminar (sonidos, etc.)")]
    public UnityEvent onTimerFinished;

    private float remaining;
    private bool finished;
    private bool paused;

    public float Remaining => remaining;
    public bool IsPaused => paused;

    // Usados por GameStateManager (por ejemplo, al abrir el menú de pausa)
    public void Pause() => paused = true;
    public void Resume() => paused = false;

    private void Start()
    {
        remaining = duration;
        UpdateText();
    }

    private void Update()
    {
        if (finished || paused) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            UpdateText();
            Finish();
            return;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (timerText == null) return;
        int total = Mathf.CeilToInt(remaining);
        timerText.text = $"{total / 60:00}:{total % 60:00}";
    }

    private void Finish()
    {
        finished = true;
        onTimerFinished?.Invoke();
        SceneManager.LoadScene(returnScene);
    }
}