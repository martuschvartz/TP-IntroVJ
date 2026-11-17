using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// La lista de pistas del recuerdo (reemplaza a la "nota en la mano").
// Con Tab se muestra / oculta. Es un Scroll View: cada renglon se instancia
// dentro del Content, que tiene un Vertical Layout Group.
// Arriba va la declaracion del sospechoso (lo que ya sabemos); abajo, las pistas
// a medida que se encuentran (no se muestran antes, para no spoilear que buscar).
public class ClueListUI : MonoBehaviour
{
    [SerializeField] private MemoryManager memoryManager;
    [Tooltip("El Scroll View entero (se prende y apaga con Tab).")]
    [SerializeField] private GameObject panel;
    [Tooltip("El Content del Scroll View (con Vertical Layout Group).")]
    [SerializeField] private Transform content;
    [Tooltip("Prefab de un renglón: un TextMeshPro.")]
    [SerializeField] private TMP_Text rowPrefab;

    private InputAction _cluesAction;

    #region Suscripcion a eventos
    private void OnEnable()
    {
        GameEvents.ClueFound += HandleClueFound;
        GameEvents.MemoryCompleted += HandleMemoryCompleted;
    }

    private void OnDisable()
    {
        GameEvents.ClueFound -= HandleClueFound;
        GameEvents.MemoryCompleted -= HandleMemoryCompleted;
    }
    #endregion

    private void Start()
    {
        _cluesAction = InputSystem.actions.FindAction("Player/Clues");
        panel.SetActive(false);

        // Primero lo que ya sabemos (la version del sospechoso).
        // La conclusion (que contradice a que) no se muestra aca: se hace en el tablero.
        SuspectData suspect = memoryManager.Suspect;
        if (suspect != null && suspect.Statements.Length > 0)
        {
            AddRow("<b>DECLARACIÓN DE " + suspect.DisplayName.ToUpper() + "</b>");
            foreach (StatementData statement in suspect.Statements)
            {
                AddRow("<i>\"" + statement.Text + "\"</i>");
            }
            AddRow(" "); // renglon vacio de separacion
        }

        // Las pistas se van agregando debajo cuando se encuentran (HandleClueFound).
        AddRow("<b>LO QUE ENCONTRÉ</b>");
    }

    private void Update()
    {
        if (_cluesAction.WasPressedThisFrame()) panel.SetActive(!panel.activeSelf);
    }

    private void HandleClueFound(MemoryClue clue)
    {
        if (Array.IndexOf(memoryManager.Clues, clue) < 0)
        {
            Debug.LogWarning("La pista '" + clue.name + "' no esta en la lista Clues del MemoryManager: no cuenta para salir del recuerdo.", clue);
        }

        AddRow("• " + clue.ListText);
    }

    private TMP_Text AddRow(string text)
    {
        TMP_Text row = Instantiate(rowPrefab, content);
        row.text = text;
        // Por si el prefab del renglon quedo guardado apagado.
        row.gameObject.SetActive(true);
        return row;
    }

    private void HandleMemoryCompleted(string memoryId)
    {
        // Al completar el recuerdo se muestra la lista con todo lo encontrado.
        panel.SetActive(true);
    }
}
