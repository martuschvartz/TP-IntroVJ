// Los hilos que puede tirar el jugador en el tablero.
// (El amarillo de los testigos no va aca: es fijo, no lo pone el jugador.)
public enum ThreadColor
{
    Contradice, // rojo: la pista contradice la declaracion
    Respalda    // azul: la pista respalda la declaracion
}
