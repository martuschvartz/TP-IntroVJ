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
    [Tooltip("Ambigua: se puede leer para los dos lados. Cualquier hilo que le pongan no cuenta como error "
             + "(y no expone ninguna mentira). Si esta tildado, Declaracion y Color se ignoran.")]
    [SerializeField] private bool ambiguous;

    public string SuspectId => suspectId;
    public bool Ambiguous => ambiguous;
    public string Text => text;
    public StatementData RelatedStatement => relatedStatement;
    public ThreadColor ExpectedColor => expectedColor;
}
