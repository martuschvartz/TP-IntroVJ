using System.Collections;
using UnityEngine;

// Espera unos segundos antes de pasar al siguiente comando.
public class WaitCommand : ICommand
{
    private readonly float _seconds;

    public WaitCommand(float seconds)
    {
        _seconds = seconds;
    }

    public IEnumerator Execute()
    {
        yield return new WaitForSeconds(_seconds);
    }
}
