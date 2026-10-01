using _Project.Scripts.Managers;
using TMPro;
using UnityEngine;

// Pesadilla inicial: pantalla en negro, la fecha, un grito, un disparo,
// "siete años después" y se pasa al despacho. Toda la secuencia se arma
// como una cola de comandos que se ejecutan de a uno.
// El ruido de fondo puede ser un AudioSource en loop en la escena.
public class NightmareSequence : MonoBehaviour
{
    [SerializeField] private CommandQueue queue;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text label;

    [SerializeField] private AudioClip scream;
    [SerializeField] private AudioClip gunshot;

    [SerializeField] private string dateText = "17 de enero de 2081";
    [SerializeField] private string afterText = "Siete años después";

    private void Start()
    {
        label.text = "";

        queue.Enqueue(new WaitCommand(1f));
        queue.Enqueue(new ShowTextCommand(label, dateText, 3f));
        queue.Enqueue(new PlayAudioCommand(audioSource, scream));
        queue.Enqueue(new PlayAudioCommand(audioSource, gunshot));
        queue.Enqueue(new WaitCommand(1.5f));
        queue.Enqueue(new ShowTextCommand(label, afterText, 3f));
        queue.Enqueue(new LoadSceneCommand(SceneAdministrator.DESPACHO));
    }
}
