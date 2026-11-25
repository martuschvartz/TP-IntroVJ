using _Project.Scripts.Managers;
using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
using UnityEngine;

// Documento de un sospechoso en el despacho. Tocarlo = entrar a su recuerdo
// Si ese recuerdo ya fue visitado, el documento no aparece (no se puede volver a entrar),
// salvo que sea un caso que se cierra desde el tablero ("Cerrar sospechoso"): ese queda
// hasta que se cierra.
[RequireComponent(typeof(Collider))]
public class MemoryDocument : MonoBehaviour, IInteractable
{
    [Tooltip("Id del recuerdo. Tiene que coincidir con el del MemoryManager de la escena. Ej: Mateo")]
    [SerializeField] private string memoryId;
    [Tooltip("Escena del recuerdo. Se entra pasando por la pantalla de carga.")]
    [SerializeField] private SceneName sceneToLoad;
    [Tooltip("Si esta prendido, el documento no desaparece al visitar el recuerdo: queda hasta que se " +
             "cierra el sospechoso desde el tablero.")]
    [SerializeField] private bool staysUntilClosed;

    private bool _opening;

    private void Start()
    {
        bool done = staysUntilClosed ? GameProgress.IsClosed(memoryId) : GameProgress.IsVisited(memoryId);
        if (done) gameObject.SetActive(false);
    }

    public CrosshairIcon Icon => CrosshairIcon.Hand;

    public void Interact()
    {
        if (_opening) return; // ya se esta entrando al recuerdo
        _opening = true;
        GameEvents.RaisePlayerLockChanged(true);
        SceneAdministrator.LoadWithLoadingScreen(sceneToLoad);
    }
}
