using _Project.Scripts.Strategy;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneChanger : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneToLoad;

    public CrosshairIcon Icon => CrosshairIcon.Hand;

    public void Interact()
    {
        SceneManager.LoadSceneAsync(sceneToLoad);
    }
}