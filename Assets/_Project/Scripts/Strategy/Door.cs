using _Project.Scripts.Strategy;
using UnityEngine;

// Va en el objeto FIJO, el que tiene el Box Collider trigger.
// Rota la bisagra referenciada, NO a sí mismo, así la zona de interacción no se mueve.
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]                       
public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform hinge;       // el objeto que realmente gira
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Sonido")]                                         
    [SerializeField] private AudioClip openSound;              
    [SerializeField] private AudioClip closeSound;             
    [SerializeField, Range(0f, 1f)] private float volume = 1f; 
    [SerializeField] private float minDistance = 100f; 
    [SerializeField] private float maxDistance = 200f; 

    private Quaternion _closedRot;
    private Quaternion _openRot;
    private bool _isOpen;
    private AudioSource _audio;

    private void Awake()                                       
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.loop = false;
        _audio.spatialBlend = 1f; 
        _audio.rolloffMode = AudioRolloffMode.Linear; // atenuación más predecible
        _audio.minDistance = minDistance;
        _audio.maxDistance = maxDistance;
    }

    private void Start()
    {
        _closedRot = hinge.localRotation;
        _openRot = _closedRot * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        Quaternion target = _isOpen ? _openRot : _closedRot;
        hinge.localRotation = Quaternion.Slerp(hinge.localRotation, target, Time.deltaTime * rotationSpeed);
    }

    public void Interact()
    {
        _isOpen = !_isOpen;

        AudioClip clip = _isOpen ? openSound : closeSound;
        if (clip != null) _audio.PlayOneShot(clip, volume);
    }
}