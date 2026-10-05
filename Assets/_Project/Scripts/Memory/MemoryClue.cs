using _Project.Scripts.Strategy;
using UnityEngine;

public enum ClueType
{
    Consistencia,   // coincide con lo que declaro el sospechoso
    Inconsistencia, // contradice lo que declaro: delata la mentira
    Trigger         // brilla; al activarlo aparece la escena oculta (la revelacion sobre vos)
}

// Va en cada objeto interactuable de un recuerdo. Es un IInteractable mas
// (Strategy): PlayerInteractor no sabe que es una pista, solo le dice Interact().
// Al encontrarla avisa por GameEvents.ClueFound; no conoce a nadie mas.
[RequireComponent(typeof(Collider))]
public class MemoryClue : MonoBehaviour, IInteractable, IFocusable
{
    [SerializeField] private ClueType type;

    [Tooltip("Texto que aparece en la lista de pistas (Tab). Ej: 'Marca de cigarrillos'")]
    [SerializeField, TextArea] private string listText;

    [Tooltip("Se prende mientras el jugador mira el objeto. Puede quedar vacío.")]
    [SerializeField] private GameObject highlight;

    [Header("Solo para Trigger")]
    [Tooltip("Brillo que queda prendido mientras el trigger se puede activar.")]
    [SerializeField] private GameObject glow;
    [SerializeField] private CommandQueue queue;
    [Tooltip("Lo que aparece al activarlo (tu figura, la caja del inyectable...). Puede quedar vacío.")]
    [SerializeField] private GameObject revealPrefab;
    [SerializeField] private Transform revealPoint;
    [Tooltip("Objetos de la escena que desaparecen al activarlo. Puede quedar vacío.")]
    [SerializeField] private GameObject[] destroyOnReveal;
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Sonido de la revelación (la llamada, el grito + disparo...). Puede quedar vacío.")]
    [SerializeField] private AudioClip revealSound;

    [Header("Cuarto paralelo (solo Trigger, opcional)")]
    [Tooltip("Nombre de la escena del cuarto paralelo (tiene que estar en Build Profiles). Si queda vacío, no hay cuarto paralelo.")]
    [SerializeField] private string parallelSceneName;
    [SerializeField] private Transform player;
    [SerializeField] private float parallelRoomSeconds = 30f;
    [Tooltip("Se apaga mientras estás en el cuarto paralelo (por ejemplo, el padre con todo el cuarto original). Puede quedar vacío.")]
    [SerializeField] private GameObject hideWhileAway;

    private bool _locked;

    public ClueType Type => type;
    public string ListText => listText;
    public bool Found { get; private set; }

    private void Start()
    {
        if (highlight != null) highlight.SetActive(false);
        UpdateGlow();
    }

    // MemoryManager bloquea el trigger hasta que se encuentra la inconsistencia.
    public void SetLocked(bool locked)
    {
        _locked = locked;
        UpdateGlow();
    }

    public void Interact()
    {
        if (Found || _locked) return;

        Found = true;
        Debug.Log("Pista encontrada: " + name + " (" + type + ")");
        if (highlight != null) highlight.SetActive(false);
        UpdateGlow();

        if (type == ClueType.Trigger) Reveal();

        GameEvents.RaiseClueFound(this);
    }

    public void OnFocus()
    {
        if (!Found && !_locked && highlight != null) highlight.SetActive(true);
    }

    public void OnLoseFocus()
    {
        if (highlight != null) highlight.SetActive(false);
    }

    // La escena oculta: se encola en la CommandQueue para que pase en orden
    // (primero aparece la figura, despues suena el audio).
    private void Reveal()
    {
        foreach (GameObject go in destroyOnReveal)
        {
            Destroy(go);
        }

        if (!string.IsNullOrEmpty(parallelSceneName))
        {
            RevealInParallelRoom();
            return;
        }

        if (revealPrefab != null) queue.Enqueue(new SpawnCommand(revealPrefab, revealPoint));
        if (revealSound != null) queue.Enqueue(new PlayAudioCommand(audioSource, revealSound));
    }

    // Te lleva al cuarto paralelo (otra escena), te deja ahi un rato y te devuelve.
    // Lo que se ve alla se arma directo en esa escena, sin revealPrefab.
    // Como todo pasa en la cola, si el trigger fue la ultima pista, MemoryManager
    // espera a que vuelvas antes de salir del recuerdo.
    private void RevealInParallelRoom()
    {
        // Sin esperar a que termine: el audio suena mientras estas en el cuarto paralelo.
        if (revealSound != null) queue.Enqueue(new PlayAudioCommand(audioSource, revealSound, false));
        queue.Enqueue(new VisitParallelSceneCommand(parallelSceneName, player, parallelRoomSeconds, hideWhileAway));
    }

    private void UpdateGlow()
    {
        if (glow != null) glow.SetActive(!Found && !_locked);
    }
}
