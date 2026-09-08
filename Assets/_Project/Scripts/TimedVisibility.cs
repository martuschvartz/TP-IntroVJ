using UnityEngine;

public class TimedVisibility : MonoBehaviour
{
    [SerializeField] private float delay = 5f;
    void Start()
    {
        Invoke(nameof(Disappear), delay);
    }

    void Update()
    {
        
    }
    
    void Disappear()
    {
        gameObject.SetActive(false);
    }
}
