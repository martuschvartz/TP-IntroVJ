using TMPro;
using UnityEngine;

// Conversacion de fondo en el despacho. Es un trigger collider: cuando el
// jugador entra (solo la primera vez), se bloquea el control, la camara hace
// zoom hacia donde viene la charla y se reproducen los dialogos con subtitulos.
//
// El zoom es una segunda CinemachineCamera (apagada al empezar) con menos FOV
// mirando hacia la zona de la charla. Al prenderla, Cinemachine hace la
// transicion sola desde la camara del jugador; al apagarla, vuelve.
[RequireComponent(typeof(Collider))]
public class CinematicZone : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Audio de la línea. Puede quedar vacío si es solo texto.")]
        public AudioClip clip;
        [TextArea] public string text;
        [Tooltip("Cuánto dura el subtítulo si no hay audio.")]
        public float seconds = 3f;
    }

    [SerializeField] private CommandQueue queue;
    [SerializeField] private GameObject zoomCamera;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text subtitleLabel;
    [SerializeField] private DialogueLine[] lines;

    private void Start()
    {
        zoomCamera.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameProgress.OfficeCinematicSeen) return;
        if (other.GetComponentInParent<FirstPersonPlayer>() == null) return;

        GameProgress.OfficeCinematicSeen = true;
        Play();
    }

    private void Play()
    {
        queue.Enqueue(new LockPlayerCommand(true));
        queue.Enqueue(new SetActiveCommand(zoomCamera, true));
        queue.Enqueue(new WaitCommand(1f));

        foreach (DialogueLine line in lines)
        {
            float seconds = line.seconds;
            if (line.clip != null)
            {
                queue.Enqueue(new PlayAudioCommand(audioSource, line.clip, false));
                seconds = line.clip.length;
            }
            queue.Enqueue(new ShowTextCommand(subtitleLabel, line.text, seconds));
        }

        queue.Enqueue(new SetActiveCommand(zoomCamera, false));
        queue.Enqueue(new WaitCommand(1f));
        queue.Enqueue(new LockPlayerCommand(false));
    }
}
