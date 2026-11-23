using System.Collections;
using _Project.Scripts.Managers;
using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

// Documento de un sospechoso en el despacho. Tocarlo = leer su testimonio
// (pantalla negra) y despues entrar a su recuerdo.
// Si ese recuerdo ya fue visitado, el documento no aparece (no se puede volver a entrar).
[RequireComponent(typeof(Collider))]
public class MemoryDocument : MonoBehaviour, IInteractable
{
    [Tooltip("Id del recuerdo. Tiene que coincidir con el del MemoryManager de la escena. Ej: Mateo")]
    [SerializeField] private string memoryId;
    [Tooltip("Escena del recuerdo. Se entra pasando por la pantalla de carga.")]
    [SerializeField] private SceneName sceneToLoad;

    [Header("Testimonio")]
    [Tooltip("Titulo de la pantalla negra. Ej: Testimonio de Matías")]
    [SerializeField] private string testimonyTitle;
    [Tooltip("Resumen de lo que dijo la persona. Se muestra antes de entrar al recuerdo.")]
    [TextArea(4, 10)] [SerializeField] private string testimony;
    [Tooltip("Segundos minimos antes de poder pasar con E (para que no se saltee sin querer).")]
    [SerializeField] private float minReadSeconds = 2f;

    private bool _opening;

    private void Start()
    {
        if (GameProgress.IsVisited(memoryId)) gameObject.SetActive(false);
    }

    public CrosshairIcon Icon => CrosshairIcon.Hand;

    public void Interact()
    {
        if (_opening) return; // ya se esta mostrando el testimonio
        _opening = true;
        StartCoroutine(ShowTestimonyAndEnter());
    }

    private IEnumerator ShowTestimonyAndEnter()
    {
        GameEvents.RaisePlayerLockChanged(true);

        if (!string.IsNullOrWhiteSpace(testimony))
        {
            TestimonyScreen.Show(testimonyTitle, testimony, out var hint);

            yield return new WaitForSeconds(minReadSeconds);
            hint.text = "Presioná E para entrar al recuerdo";

            InputAction interact = InputSystem.actions.FindAction("Player/Interact");
            while (!interact.WasPressedThisFrame()) yield return null;
        }

        SceneAdministrator.LoadWithLoadingScreen(sceneToLoad);
    }
}
