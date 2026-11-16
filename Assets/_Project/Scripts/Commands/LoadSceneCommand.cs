using System.Collections;
using _Project.Scripts.Managers;
using _Project.Scripts.Utility;

// Cambia de escena como un paso más de una secuencia. No carga por su cuenta:
// le pide a SceneAdministrator, que es el único que sabe cargar escenas.
public class LoadSceneCommand : ICommand
{
    private readonly SceneName _scene;
    private readonly bool _withLoadingScreen;

    public LoadSceneCommand(SceneName scene, bool withLoadingScreen = false)
    {
        _scene = scene;
        _withLoadingScreen = withLoadingScreen;
    }

    public IEnumerator Execute()
    {
        if (_withLoadingScreen) SceneAdministrator.LoadWithLoadingScreen(_scene);
        else SceneAdministrator.Load(_scene);

        // Es el último paso: al cambiar de escena, la cola se destruye con ella.
        yield break;
    }
}
