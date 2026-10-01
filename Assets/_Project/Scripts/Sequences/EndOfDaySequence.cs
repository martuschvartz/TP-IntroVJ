using _Project.Scripts.Managers;
using UnityEngine;

// Pantalla de "Fin del dia": se muestra unos segundos y vuelve al despacho.
// El texto lo pone la escena (un TextMeshPro en el Canvas).
public class EndOfDaySequence : MonoBehaviour
{
    [SerializeField] private CommandQueue queue;
    [SerializeField] private float seconds = 3f;

    private void Start()
    {
        queue.Enqueue(new WaitCommand(seconds));
        queue.Enqueue(new LoadSceneCommand(SceneAdministrator.DESPACHO));
    }
}
