using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// La lista de pistas del recuerdo (reemplaza a la "nota en la mano").
// Con Tab se muestra / oculta. Es un Scroll View: cada pista es un renglon
// instanciado dentro del Content, que tiene un Vertical Layout Group.
public class ClueListUI : MonoBehaviour
{
    [SerializeField] private MemoryManager memoryManager;
    [Tooltip("El Scroll View entero (se prende y apaga con Tab).")]
    [SerializeField] private GameObject panel;
    [Tooltip("El Content del Scroll View (con Vertical Layout Group).")]
    [SerializeField] private Transform content;
    [Tooltip("Prefab de un renglón: un TextMeshPro.")]
    [SerializeField] private TMP_Text rowPrefab;

    private readonly Dictionary<MemoryClue, TMP_Text> _rows = new Dictionary<MemoryClue, TMP_Text>();
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

        foreach (MemoryClue clue in memoryManager.Clues)
        {
            TMP_Text row = Instantiate(rowPrefab, content);
            row.text = "[  ] " + clue.ListText;

            // El trigger no aparece en la lista hasta encontrarlo, para no spoilear.
            row.gameObject.SetActive(clue.Type != ClueType.Trigger);
            _rows.Add(clue, row);
        }
    }

    private void Update()
    {
        if (_cluesAction.WasPressedThisFrame()) panel.SetActive(!panel.activeSelf);
    }

    private void HandleClueFound(MemoryClue clue)
    {
        TMP_Text row = _rows[clue];
        row.text = "[x] " + clue.ListText;
        row.gameObject.SetActive(true);
    }

    private void HandleMemoryCompleted(string memoryId)
    {
        // Al completar el recuerdo se muestra la lista con todo tildado.
        panel.SetActive(true);
    }
}
