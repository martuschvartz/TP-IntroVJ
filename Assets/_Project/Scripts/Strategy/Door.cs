using _Project.Scripts.Strategy;
using UnityEngine;

// Va en el objeto FIJO, el que tiene el Box Collider trigger.
// Rota la bisagra referenciada, NO a sí mismo, así la zona de interacción no se mueve.
[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform hinge;       // el objeto que realmente gira
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float rotationSpeed = 20f;

    private Quaternion _closedRot;
    private Quaternion _openRot;
    private bool _isOpen;

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

    public void Interact() => _isOpen = !_isOpen;
}