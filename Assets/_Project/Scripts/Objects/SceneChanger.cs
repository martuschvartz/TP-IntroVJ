using _Project.Scripts.Strategy;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneChanger : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneToLoad;
    
    public void Interact()
    {
        Debug.Log("Interact de la nota");
        SceneManager.LoadScene(sceneToLoad);
    }
}