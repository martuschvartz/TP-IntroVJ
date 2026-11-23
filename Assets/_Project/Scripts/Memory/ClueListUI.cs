using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// La lista de pistas del recuerdo (reemplaza a la "nota en la mano").
// Con Tab se muestra / oculta. Es un Scroll View: cada renglon se instancia
// dentro del Content, que tiene un Vertical Layout Group.
// Arriba va la declaracion del sospechoso (lo que ya sabemos); abajo, las pistas
// a medida que se encuentran (no se muestran antes, para no spoilear que buscar),
// con un contador "encontradas/total".
// Mientras esta abierta el jugador queda quieto y se ve el cursor (como mirar la libreta),
// asi se puede usar el boton de volver al despacho antes de encontrar todo.
public class ClueListUI : MonoBehaviour
{
    [SerializeField] private MemoryManager memoryManager;
    [Tooltip("El Scroll View entero (se prende y apaga con Tab).")]
    [SerializeField] private GameObject panel;
    [Tooltip("El Content del Scroll View (con Vertical Layout Group).")]
    [SerializeField] private Transform content;
    [Tooltip("Prefab de un renglón: un TextMeshPro.")]
    [SerializeField] private TMP_Text rowPrefab;

    [Header("Volver antes")]
    [Tooltip("Botón 'Volver al despacho'. Aparece cuando se llega al mínimo de pistas del MemoryManager.")]
    [SerializeField] private Button exitButton;
    [Tooltip("Aviso de que quedan pistas sin encontrar (ej. 'Todavía hay cosas por encontrar'). Puede quedar vacío.")]
    [SerializeField] private GameObject missingCluesWarning;

    private InputAction _cluesAction;
    private TMP_Text _counterRow;

    // Bloqueado por otro (una cinematica, la salida del recuerdo): ahi Tab no abre la lista.
    private bool _lockedByOthers;
    // true mientras la lista es la que tiene bloqueado al jugador.
    private bool _ownsLock;
    // Para ignorar el evento que dispara la propia lista al bloquear / desbloquear.
    private bool _raisingLock;
    private bool _completed;

    #region Suscripcion a eventos
    private void OnEnable()
    {
        GameEvents.ClueFound += HandleClueFound;
        GameEvents.MemoryCompleted += HandleMemoryCompleted;
        GameEvents.PlayerLockChanged += HandlePlayerLockChanged;
    }

    private void OnDisable()
    {
        GameEvents.ClueFound -= HandleClueFound;
        GameEvents.MemoryCompleted -= HandleMemoryCompleted;
        GameEvents.PlayerLockChanged -= HandlePlayerLockChanged;
    }
    #endregion

    private void Start()
    {
        _cluesAction = InputSystem.actions.FindAction("Player/Clues");
        panel.SetActive(false);

        if (exitButton != null) exitButton.onClick.AddListener(ExitEarly);

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
        _counterRow = AddRow("");
        RefreshCounter();
        RefreshExit();
    }

    private void Update()
    {
        if (!_cluesAction.WasPressedThisFrame()) return;

        if (panel.activeSelf) Close();
        else if (!_lockedByOthers && !_completed) Open();
    }

    private void Open()
    {
        panel.SetActive(true);
        SetPlayerLocked(true);
        RefreshExit();
    }

    private void Close()
    {
        panel.SetActive(false);
        if (_ownsLock) SetPlayerLocked(false);
    }

    private void SetPlayerLocked(bool locked)
    {
        _ownsLock = locked;

        _raisingLock = true;
        GameEvents.RaisePlayerLockChanged(locked);
        _raisingLock = false;

        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }

    private void HandlePlayerLockChanged(bool locked)
    {
        if (_raisingLock) return;
        _lockedByOthers = locked;
    }

    private void HandleClueFound(MemoryClue clue)
    {
        if (Array.IndexOf(memoryManager.Clues, clue) < 0)
        {
            Debug.LogWarning("La pista '" + clue.name + "' no esta en la lista Clues del MemoryManager: no cuenta para salir del recuerdo.", clue);
        }

        AddRow("• " + clue.ListText);
        RefreshCounter();
        RefreshExit();
    }

    private void RefreshCounter()
    {
        _counterRow.text = "<b>LO QUE ENCONTRÉ (" + memoryManager.FoundCount + "/" + memoryManager.TotalClues + ")</b>";
    }

    // El boton aparece con el minimo de pistas; si ya estan todas, el recuerdo sale solo.
    private void RefreshExit()
    {
        bool canExit = !_completed && memoryManager.CanExitEarly && !memoryManager.AllCluesFound;
        if (exitButton != null) exitButton.gameObject.SetActive(canExit);
        if (missingCluesWarning != null) missingCluesWarning.SetActive(canExit);
    }

    private void ExitEarly()
    {
        if (!memoryManager.CanExitEarly) return;

        // El cursor vuelve a quedar bloqueado; el jugador queda quieto (lo bloquea Exit) hasta el cambio de escena.
        _ownsLock = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        panel.SetActive(false);
        memoryManager.Exit();
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
        // Al completar el recuerdo se muestra la lista con todo lo encontrado (sin cursor:
        // el recuerdo sale solo despues de unos segundos).
        _completed = true;
        RefreshExit();
        panel.SetActive(true);
    }
}
