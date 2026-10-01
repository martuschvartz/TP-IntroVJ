using System.Collections;
using UnityEngine.SceneManagement;

// Carga una escena de forma asincronica.
public class LoadSceneCommand : ICommand
{
    private readonly string _sceneName;

    public LoadSceneCommand(string sceneName)
    {
        _sceneName = sceneName;
    }

    public IEnumerator Execute()
    {
        yield return SceneManager.LoadSceneAsync(_sceneName);
    }
}
