using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Patron Event Queue: una cola de comandos que se ejecutan de a uno, en orden.
// Quien encola no espera ni sabe cuando se va a ejecutar su comando: solo lo
// agrega. Se usa para la pesadilla, la conversacion del despacho y el evento
// del trigger de cada recuerdo.
public class CommandQueue : MonoBehaviour
{
    private readonly Queue<ICommand> _commands = new Queue<ICommand>();
    private bool _isRunning;

    // true mientras quede algo por ejecutar.
    public bool IsBusy => _isRunning || _commands.Count > 0;

    public void Enqueue(ICommand command)
    {
        _commands.Enqueue(command);
        if (!_isRunning) StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        _isRunning = true;
        while (_commands.Count > 0)
        {
            ICommand command = _commands.Dequeue();
            yield return command.Execute();
        }
        _isRunning = false;
    }
}
