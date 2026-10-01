using System.Collections;

// Bloquea o desbloquea al jugador avisando por GameEvents.
public class LockPlayerCommand : ICommand
{
    private readonly bool _locked;

    public LockPlayerCommand(bool locked)
    {
        _locked = locked;
    }

    public IEnumerator Execute()
    {
        GameEvents.RaisePlayerLockChanged(_locked);
        yield break;
    }
}
