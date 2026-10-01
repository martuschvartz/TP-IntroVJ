using System.Collections;

// Patron Command: cada accion de una secuencia (reproducir un audio, mostrar
// un texto, esperar, cargar una escena...) es un objeto con un Execute().
// Execute es una corrutina para que la accion pueda "tardar" (por ejemplo,
// esperar a que termine el audio) antes de que la cola pase a la siguiente.
public interface ICommand
{
    IEnumerator Execute();
}
