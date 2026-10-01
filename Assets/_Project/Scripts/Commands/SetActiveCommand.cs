using System.Collections;
using UnityEngine;

// Prende o apaga un GameObject (por ejemplo, la camara de zoom de la cinematica).
public class SetActiveCommand : ICommand
{
    private readonly GameObject _target;
    private readonly bool _active;

    public SetActiveCommand(GameObject target, bool active)
    {
        _target = target;
        _active = active;
    }

    public IEnumerator Execute()
    {
        _target.SetActive(_active);
        yield break;
    }
}
