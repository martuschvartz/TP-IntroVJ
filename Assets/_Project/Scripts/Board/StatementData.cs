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

    // El veredicto de la declaracion. El fragmento se desbloquea cuando todas las
    // mentiras del sospechoso estan expuestas (al menos una pista suya con hilo rojo).
    [Tooltip("Tildado = esta declaracion es mentira. Para resolver el caso hay que conectarle " +
             "al menos una pista con hilo rojo (Contradice). Destildado = es verdad: no hace falta conectarla.")]
    [SerializeField] private bool isLie;

    public string SuspectId => suspectId;
    public string Text => text;
    public bool IsLie => isLie;
}
