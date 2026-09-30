namespace _Project.Scripts.Strategy
{
    // Para objetos que reaccionan cuando el jugador los mira (resaltarse, mostrar un texto, etc.).
    // Es independiente de IInteractable: un objeto puede ser uno, el otro o ambos.
    public interface IFocusable
    {
        void OnFocus();
        void OnLoseFocus();
    }
}