using System.Collections.Generic;
using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
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

    [Tooltip("Los datos de la pista (texto y respuesta correcta). Se crea con Create > What Lies Behind > Pista.")]
    [SerializeField] private ClueData data;

    [Tooltip("Resaltado viejo (ej: las copias negras 'outline'). Ya no se prende: ahora brilla con InteractableGlow. " +
             "Queda siempre apagado y no se usa para el brillo.")]
    [SerializeField] private GameObject highlight;

    [Tooltip("Lo que brilla al mirar la pista. Vacio = se busca solo: las mallas del objeto, " +
             "o si no tiene (ej: una pista que es solo un collider sobre las botellas), las mallas que quedan dentro de sus colliders.")]
    [SerializeField] private Renderer[] glowRenderers;

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
    [Tooltip("Si está apagado, no hay cuarto paralelo.")]
    [SerializeField] private bool hasParallelScene;
    [Tooltip("Escena del cuarto paralelo (tiene que estar en Build Profiles).")]
    [SerializeField] private SceneName parallelScene;
    [SerializeField] private Transform player;
    [SerializeField] private float parallelRoomSeconds = 30f;
    [Tooltip("Se apaga mientras estás en el cuarto paralelo (por ejemplo, el padre con todo el cuarto original). Puede quedar vacío.")]
    [SerializeField] private GameObject hideWhileAway;

    private bool _locked;
    private Renderer[] _resolvedGlowRenderers;

    public ClueType Type => type;
    public ClueData Data => data;
    public string ListText => data != null ? data.Text : name;
    public bool Found { get; private set; }

    // Las mallas que brillan cuando se puede agarrar (las usa PlayerInteractor).
    public IReadOnlyList<Renderer> GlowRenderers
    {
        get
        {
            if (_resolvedGlowRenderers == null) _resolvedGlowRenderers = ResolveGlowRenderers();
            return _resolvedGlowRenderers;
        }
    }

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

    // Ya encontrada o bloqueada no hace nada al apretar E -> la mira vuelve al puntito.
    public CrosshairIcon Icon => (Found || _locked) ? CrosshairIcon.Dot : CrosshairIcon.Clue;

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

    // Al volver a entrar a un recuerdo: la pista ya se habia encontrado en una visita anterior.
    // Queda encontrada sin volver a avisar ni a reproducir la revelacion (ni el cuarto paralelo
    // ni el audio): solo queda la escena como termino (lo que desaparecio, la figura que aparecio).
    public void RestoreFound()
    {
        if (Found) return;
        Found = true;
        UpdateGlow();

        if (type != ClueType.Trigger) return;
        foreach (GameObject go in destroyOnReveal)
        {
            if (go != null) Destroy(go);
        }
        if (!hasParallelScene && revealPrefab != null && revealPoint != null)
        {
            Instantiate(revealPrefab, revealPoint.position, revealPoint.rotation);
        }
    }

    // El brillo al mirarla lo pone PlayerInteractor (InteractableGlow).
    public void OnFocus()
    {
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

        if (hasParallelScene)
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
        queue.Enqueue(new VisitParallelSceneCommand(parallelScene, player, parallelRoomSeconds, hideWhileAway));
    }

    private Renderer[] ResolveGlowRenderers()
    {
        List<Renderer> result = new List<Renderer>();
        if (glowRenderers != null)
        {
            foreach (Renderer r in glowRenderers)
            {
                if (r != null) result.Add(r);
            }
            if (result.Count > 0) return result.ToArray();
        }

        // 1) Las mallas del propio objeto (menos el highlight y el brillo del trigger).
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            if (IsMesh(r) && !IsEffect(r.transform)) result.Add(r);
        }
        if (result.Count > 0) return result.ToArray();

        // 2) No tiene mallas propias: las que entran enteras dentro de alguno de sus colliders.
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        if (colliders.Length == 0) return result.ToArray();
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!IsMesh(r) || r.gameObject.scene != gameObject.scene) continue;
            if (IsEffect(r.transform) || r.GetComponentInParent<MemoryClue>() != null) continue;
            Bounds rb = r.bounds;
            foreach (Collider c in colliders)
            {
                Bounds cb = c.bounds;
                cb.Expand(cb.size * 0.1f);
                if (cb.Contains(rb.min) && cb.Contains(rb.max)) { result.Add(r); break; }
            }
        }
        if (result.Count == 0) Debug.LogWarning("La pista '" + name + "' no tiene mallas para brillar: asignalas en Glow Renderers.", this);
        return result.ToArray();
    }

    private static bool IsMesh(Renderer r) => r is MeshRenderer || r is SkinnedMeshRenderer;

    private bool IsEffect(Transform t) =>
        (highlight != null && t.IsChildOf(highlight.transform)) || (glow != null && t.IsChildOf(glow.transform));

    private void UpdateGlow()
    {
        if (glow != null) glow.SetActive(!Found && !_locked);
    }
}
