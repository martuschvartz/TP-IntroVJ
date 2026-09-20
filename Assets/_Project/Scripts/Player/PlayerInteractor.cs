using _Project.Scripts.Strategy;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float range = 50f;
    [SerializeField] private KeyCode key = KeyCode.E;

    private void Update()
    {
        if (!Input.GetKeyDown(key)) return;
        
        Debug.Log("E apretada");

        // Lanzamos un rayo desde la cámara hacia donde estás mirando.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
        {
            Debug.Log("Le pegué a: " + hit.collider.name);
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            interactable?.Interact(); // si lo que tocamos es interactuable, lo activamos
        }
    }
}