using _Project.Scripts.Utility;
using TMPro;
using UnityEngine;

// Pesadilla inicial: pantalla en negro, un "¡No, esperá!", el disparo (que corta
// la frase), silencio y se pasa al despacho. Toda la secuencia se arma como una
// cola de comandos que se ejecutan de a uno.
// El ruido de fondo puede ser un AudioSource en loop en la escena.
public class NightmareSequence : MonoBehaviour
{
    [SerializeField] private CommandQueue queue;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text label;

    [SerializeField] private AudioClip gunshot;

    [SerializeField] private string subtitle = "¡No, esperá!";
    [Tooltip("Cuanto se ve el subtitulo antes del disparo. Al sonar el disparo, el texto se borra.")]
    [SerializeField] private float subtitleSeconds = 0.8f;
    [Tooltip("Silencio en negro antes del subtitulo.")]
    [SerializeField] private float secondsBefore = 1f;
    [Tooltip("Silencio en negro despues del disparo, antes de pasar al despacho.")]
    [SerializeField] private float secondsAfter = 2f;

    private void Start()
    {
        label.text = "";

        queue.Enqueue(new WaitCommand(secondsBefore));
        queue.Enqueue(new ShowTextCommand(label, subtitle, subtitleSeconds));
        queue.Enqueue(new PlayAudioCommand(audioSource, gunshot, false));
        queue.Enqueue(new WaitCommand(secondsAfter));
        queue.Enqueue(new LoadSceneCommand(SceneName.Despacho, true));
    }
}
