using _Project.Scripts.Managers;
using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
using UnityEngine;

// Documento de un sospechoso en el despacho. Tocarlo = entrar a su recuerdo.
// Si ese recuerdo ya fue visitado, el documento no aparece (no se puede volver a entrar).
[RequireComponent(typeof(Collider))]
public class MemoryDocument : MonoBehaviour, IInteractable
{
    [Tooltip("Id del recuerdo. Tiene que coincidir con el del MemoryManager de la escena. Ej: Mateo")]
    [SerializeField] private string memoryId;
    [Tooltip("Escena del recuerdo. Se entra pasando por la pantalla de carga.")]
    [SerializeField] private SceneName sceneToLoad;

    private void Start()
    {
        if (GameProgress.IsVisited(memoryId)) gameObject.SetActive(false);
    }

    public CrosshairIcon Icon => CrosshairIcon.Hand;

    public void Interact()
    {
        SceneAdministrator.LoadWithLoadingScreen(sceneToLoad);
    }
}
