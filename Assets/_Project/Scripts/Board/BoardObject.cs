using _Project.Scripts.Strategy;
using UnityEngine;

// El corcho 3D del despacho. Interactuar con el abre el tablero en modo
// "solo mirar" (lo que se armo al final de los dias anteriores).
[RequireComponent(typeof(Collider))]
public class BoardObject : MonoBehaviour, IInteractable
{
    [SerializeField] private BoardUI board;

    public void Interact()
    {
        board.Open(false);
    }
}
