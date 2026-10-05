using System.Collections;
using UnityEngine;

// Mueve al jugador de golpe a una posicion (por ejemplo, al cuarto paralelo y de vuelta).
public class TeleportCommand : ICommand
{
    private readonly Transform _player;
    private readonly Vector3 _position;
    private readonly Quaternion _rotation;

    public TeleportCommand(Transform player, Vector3 position, Quaternion rotation)
    {
        _player = player;
        _position = position;
        _rotation = rotation;
    }

    public IEnumerator Execute()
    {
        // El CharacterController pisa la posicion si esta prendido: se apaga para moverlo.
        CharacterController controller = _player.GetComponent<CharacterController>();
        controller.enabled = false;
        _player.SetPositionAndRotation(_position, _rotation);
        controller.enabled = true;
        yield break;
    }
}
