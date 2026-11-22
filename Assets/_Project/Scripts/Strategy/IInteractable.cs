using _Project.Scripts.Utility;

namespace _Project.Scripts.Strategy
{
    public interface IInteractable
    {
        // Ícono que muestra la mira del HUD al mirar este objeto.
        CrosshairIcon Icon { get; }

        void Interact();
    }
}
