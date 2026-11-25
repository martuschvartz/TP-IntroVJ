using _Project.Scripts.Managers;
using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
using UnityEngine;

// Documento de un sospechoso en el despacho. Tocarlo = entrar a su recuerdo.
// Se puede volver a entrar las veces que se quiera (las pistas ya encontradas quedan
// encontradas, ver MemoryManager.Awake). Solo desaparece cuando se cierra el sospechoso
// desde el tablero ("Cerrar sospechoso").
[RequireComponent(typeof(Collider))]
public class MemoryDocument : MonoBehaviour, IInteractable
{
    [Tooltip("Id del recuerdo. Tiene que coincidir con el del MemoryManager de la escena y con el " +
             "Id del sospechoso. Ej: Mateo")]
    [SerializeField] private string memoryId;
    [Tooltip("Escena del recuerdo. Se entra pasando por la pantalla de carga.")]
    [SerializeField] private SceneName sceneToLoad;

    private bool _opening;

    private void Start()
    {
        if (GameProgress.IsClosed(memoryId)) gameObject.SetActive(false);
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
