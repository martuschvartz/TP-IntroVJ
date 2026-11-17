// Un hilo que tiro el jugador en el tablero: pista <-> declaracion, con un color.
public class Connection
{
    public ClueData Clue { get; }
    public StatementData Statement { get; }
    public ThreadColor Color { get; }

    public Connection(ClueData clue, StatementData statement, ThreadColor color)
    {
        Clue = clue;
        Statement = statement;
        Color = color;
    }

    public bool Joins(ClueData clue, StatementData statement) => Clue == clue && Statement == statement;
}
