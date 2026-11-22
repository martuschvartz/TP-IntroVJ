using _Project.Scripts.Utility;
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

    [Header("Fragmento")]
    [Tooltip("Si resolves bien su tablero, ves esta escena (la noche desde sus ojos).")]
    [SerializeField] private bool hasFragment = true;
    [SerializeField] private SceneName fragmentScene = SceneName.LivingParalelo;

    public string Id => id;
    public string DisplayName => displayName;
    public StatementData[] Statements => statements;
    public bool HasFragment => hasFragment;
    public SceneName FragmentScene => fragmentScene;
}
