using UnityEngine;

// Una declaracion de un sospechoso (lo que dijo que paso).
// Es un asset: vive fuera de las escenas, asi que se puede guardar en GameProgress.
[CreateAssetMenu(fileName = "Declaracion", menuName = "What Lies Behind/Declaracion")]
public class StatementData : ScriptableObject
{
    [Tooltip("Id del sospechoso. Ej: Mateo")]
    [SerializeField] private string suspectId;

    [Tooltip("Lo que se lee en la tarjeta. Ej: 'A la 1 me desperte y la casa estaba revuelta'")]
    [SerializeField, TextArea] private string text;

    public string SuspectId => suspectId;
    public string Text => text;
}
