using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
using UnityEngine;

// El 4º documento del despacho: tu propio caso. Solo aparece cuando ya
// visitaste los 3 recuerdos. Al tocarlo abre el panel de decision.
// Los botones del panel ("Confesar" / "No confesar") llaman directo a
// SceneAdministrator.LoadFinalConfesar (escena Carcel) / LoadFinalNoHacerNada (escena "No carcel")
// desde el Inspector. La estetica del panel la arma EndingDecisionPanel.
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

    public CrosshairIcon Icon => CrosshairIcon.Hand;

    public void Interact()
    {
        decisionPanel.SetActive(true);
        GameEvents.RaisePlayerLockChanged(true);

        // Mostrar el mouse para poder hacer click en los botones.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
