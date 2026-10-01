using _Project.Scripts.Strategy;
using UnityEngine;
using UnityEngine.SceneManagement;

// Documento de un sospechoso en el despacho. Tocarlo = entrar a su recuerdo.
// Si ese recuerdo ya fue visitado, el documento no aparece (no se puede volver a entrar).
[RequireComponent(typeof(Collider))]
public class MemoryDocument : MonoBehaviour, IInteractable
{
    [Tooltip("Id del recuerdo. Tiene que coincidir con el del MemoryManager de la escena. Ej: Mateo")]
    [SerializeField] private string memoryId;
    [Tooltip("Nombre de la escena del recuerdo. Ej: RecuerdoMateo")]
    [SerializeField] private string sceneToLoad;

    private void Start()
    {
        if (GameProgress.IsVisited(memoryId)) gameObject.SetActive(false);
    }

    public void Interact()
    {
        SceneManager.LoadSceneAsync(sceneToLoad);
    }
}
