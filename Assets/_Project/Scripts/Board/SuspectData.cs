using UnityEngine;

// Un sospechoso y lo que declaro. Lo usan la lista del Tab (para mostrar su
// version mientras investigas el recuerdo) y el tablero (tarjetas de declaraciones).
[CreateAssetMenu(fileName = "Sospechoso", menuName = "What Lies Behind/Sospechoso")]
public class SuspectData : ScriptableObject
{
    [Tooltip("Id del sospechoso. Igual que el de sus pistas y declaraciones. Ej: Mateo")]
    [SerializeField] private string id;

    [Tooltip("Nombre que se ve en pantalla. Ej: Mateo")]
    [SerializeField] private string displayName;

    [Tooltip("Lo que declaro. Cada una es un asset de Declaracion.")]
    [SerializeField] private StatementData[] statements;

    public string Id => id;
    public string DisplayName => displayName;
    public StatementData[] Statements => statements;
}
