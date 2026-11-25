using UnityEngine;

// Sonido de lapiz en la libreta cuando se encuentra una pista.
public class ClueFoundSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pencil;
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.1f;

    private void OnEnable() => GameEvents.ClueFound += HandleClueFound;

    private void OnDisable() => GameEvents.ClueFound -= HandleClueFound;

    private void HandleClueFound(MemoryClue clue)
    {
        if (audioSource == null || pencil == null) return;

        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(pencil);
    }
}
