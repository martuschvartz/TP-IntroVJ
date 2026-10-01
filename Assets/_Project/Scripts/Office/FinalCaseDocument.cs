using _Project.Scripts.Strategy;
using UnityEngine;

// El 4º documento del despacho: tu propio caso. Solo aparece cuando ya
// visitaste los 3 recuerdos. Al tocarlo abre el panel de decision.
// Los botones del panel ("Confesar" / "No hacer nada") llaman directo a
// SceneAdministrator.LoadFinalConfesar / LoadFinalNoHacerNada desde el Inspector.
[RequireComponent(typeof(Collider))]
public class FinalCaseDocument : MonoBehaviour, IInteractable
{
    [Tooltip("Panel de UI con los dos botones. Arranca apagado.")]
    [SerializeField] private GameObject decisionPanel;

    private void Start()
    {
        decisionPanel.SetActive(false);
        gameObject.SetActive(GameProgress.AllVisited);
    }

    public void Interact()
    {
        decisionPanel.SetActive(true);
        GameEvents.RaisePlayerLockChanged(true);

        // Mostrar el mouse para poder hacer click en los botones.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
