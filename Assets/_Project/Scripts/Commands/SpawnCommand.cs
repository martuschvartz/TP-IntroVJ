using System.Collections;
using UnityEngine;

// Instancia un prefab en un punto de la escena (lo que "aparece" al activar un trigger).
public class SpawnCommand : ICommand
{
    private readonly GameObject _prefab;
    private readonly Transform _point;

    public SpawnCommand(GameObject prefab, Transform point)
    {
        _prefab = prefab;
        _point = point;
    }

    public IEnumerator Execute()
    {
        Object.Instantiate(_prefab, _point.position, _point.rotation);
        yield break;
    }
}
