using UnityEngine;

// Los datos de una pista: lo que se ve en la lista y en el tablero, y la respuesta correcta.
// El MemoryClue de la escena apunta a uno de estos. Como es un asset, sobrevive
// al cambio de escena (el MemoryClue no).
[CreateAssetMenu(fileName = "Pista", menuName = "What Lies Behind/Pista")]
public class ClueData : ScriptableObject
{
    [Tooltip("Id del sospechoso. Ej: Mateo")]
    [SerializeField] private string suspectId;

    [Tooltip("Lo que se lee en la lista (Tab) y en la tarjeta. Que no spoilee. Ej: 'Reloj de pared: marca la 01:30'")]
    [SerializeField, TextArea] private string text;

    // La respuesta: una pista no es "inconsistente" sola, sino respecto de una declaracion.
    [Header("Respuesta correcta")]
    [Tooltip("La declaracion con la que se conecta esta pista.")]
    [SerializeField] private StatementData relatedStatement;
    [Tooltip("Con que hilo se conecta.")]
    [SerializeField] private ThreadColor expectedColor;

    public string SuspectId => suspectId;
    public string Text => text;
    public StatementData RelatedStatement => relatedStatement;
    public ThreadColor ExpectedColor => expectedColor;
}
