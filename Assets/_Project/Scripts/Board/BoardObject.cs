using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
using UnityEngine;

// El corcho 3D del despacho. Interactuar con el abre el tablero acercado a la
// zona del corcho que el jugador estaba mirando.
[RequireComponent(typeof(Collider))]
public class BoardObject : MonoBehaviour, IInteractable
{
    [SerializeField] private BoardUI board;

    public CrosshairIcon Icon => CrosshairIcon.Hand;

    public void Interact()
    {
        // El mismo rayo que la mira (centro de la pantalla), pero solo contra el corcho:
        // da el punto exacto donde se estaba mirando.
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (GetComponent<Collider>().Raycast(ray, out RaycastHit hit, cam.farClipPlane))
            {
                board.OpenAt(hit.point);
                return;
            }
        }
        board.Open();
    }
}
