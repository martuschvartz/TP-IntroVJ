using System.Collections.Generic;
using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// El tablero de investigacion. Las tarjetas y los hilos viven en un canvas World Space
// apoyado sobre el corcho 3D del despacho, asi se ven siempre (aunque no lo estes usando).
// "Abrir" el tablero = prender una CinemachineCamera que mira al corcho (Cinemachine hace
// la transicion), liberar el cursor y mostrar los botones. Tiene dos modos:
// - Edicion (fin del dia): se abre solo al volver de un recuerdo. Se mueven
//   tarjetas, se tiran hilos y se cierra con "Finalizar investigacion por hoy"
//   (el caso sigue abierto) o con "Cerrar sospechoso" (se cierra el caso del
//   recuerdo que se acaba de visitar; con suficientes contradicciones se ve su fragmento).
// - Solo mirar: se abre desde el corcho 3D (E). No se puede tocar nada.
// Todo lo que se ve sale de GameProgress, asi el tablero sobrevive al cambio de escena.
public class BoardUI : MonoBehaviour
{
    [Header("Corcho (canvas World Space)")]
    [Tooltip("Capa donde se dibujan los hilos (va debajo de Cards).")]
    [SerializeField] private RectTransform threadsLayer;
    [Tooltip("Capa donde aparecen las tarjetas.")]
    [SerializeField] private RectTransform cardsLayer;
    [SerializeField] private BoardCard cardPrefab;
    [SerializeField] private BoardThread threadPrefab;

    [Header("Al abrir el tablero")]
    [Tooltip("La CinemachineCamera que mira al corcho. Arranca apagada.")]
    [SerializeField] private GameObject boardCamera;
    [Tooltip("El canvas de los botones (Overlay). Solo se ve con el tablero abierto.")]
    [SerializeField] private GameObject controls;
    [Tooltip("Lo que se esconde con el tablero abierto para que no tape el corcho (el cuerpo del jugador).")]
    [SerializeField] private Renderer[] hideWhileOpen;

    [Header("Botones")]
    [Tooltip("Lo que solo se ve en modo edicion (botones de hilo y Finalizar).")]
    [SerializeField] private GameObject editControls;
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button finishButton;
    [Tooltip("Cierra el caso del recuerdo recien visitado. Si se deja vacio, se crea solo copiando el de Finalizar.")]
    [SerializeField] private Button closeSuspectButton;
    [SerializeField] private string closeSuspectLabel = "Cerrar sospechoso";
    [Tooltip("Solo en modo mirar.")]
    [SerializeField] private Button closeButton;

    [Header("Datos")]
    [Tooltip("Todos los sospechosos. Se muestran las declaraciones de los que ya se visitaron.")]
    [SerializeField] private SuspectData[] suspects;

    [Header("Colores")]
    [SerializeField] private Color clueCardColor = new Color(0.96f, 0.94f, 0.88f);
    [SerializeField] private Color statementCardColor = new Color(1f, 0.93f, 0.6f);
    [SerializeField] private Color selectedCardColor = new Color(0.6f, 0.85f, 1f);
    [SerializeField] private Color redThreadColor = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color blueThreadColor = new Color(0.15f, 0.35f, 0.85f);

    [Header("Ubicacion inicial de las tarjetas")]
    [SerializeField] private float rowSpacing = 180f;

    [Header("Tamaño en el corcho")]
    [Tooltip("Escala de tarjetas e hilos con el tablero cerrado (mirando desde el despacho). " +
             "0.33 = el tablero entero entra en un tercio del ancho del corcho. Abierto se ven a tamaño normal (1).")]
    [Range(0.1f, 1f)]
    [SerializeField] private float closedScale = 0.33f;
    [Tooltip("Hacia donde se achican, en coordenadas normalizadas del corcho (0,0 = abajo a la izquierda). " +
             "(0, 0.5) = pegadas al borde izquierdo, centradas en altura.")]
    [SerializeField] private Vector2 closedFocus = new Vector2(0f, 0.5f);
    [Tooltip("Que tan rapido cambian de tamaño al abrir/cerrar el tablero.")]
    [SerializeField] private float scaleSpeed = 5f;

    private readonly Dictionary<ScriptableObject, BoardCard> _cards = new Dictionary<ScriptableObject, BoardCard>();
    private readonly List<BoardThread> _threads = new List<BoardThread>();
    private BoardCard _selected;
    private Vector3 _cardsLayerPosition;
    private Vector3 _threadsLayerPosition;
    private ThreadColor _color = ThreadColor.Contradice;

    public bool Editable { get; private set; }
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        boardCamera.SetActive(false);
        controls.SetActive(false);
        _cardsLayerPosition = cardsLayer.localPosition;
        _threadsLayerPosition = threadsLayer.localPosition;
        redButton.onClick.AddListener(() => SelectColor(ThreadColor.Contradice));
        blueButton.onClick.AddListener(() => SelectColor(ThreadColor.Respalda));
        finishButton.onClick.AddListener(FinishDay);
        if (closeSuspectButton == null) closeSuspectButton = CreateCloseSuspectButton();
        closeSuspectButton.onClick.AddListener(CloseSuspect);
        closeButton.onClick.AddListener(Close);
    }

    private void Start()
    {
        // El corcho muestra siempre lo que se fue armando.
        Build();

        // Volvimos de un recuerdo: es el fin del dia.
        if (GameProgress.EndOfDayPending) Open(true);

        // Arranca directo en el tamaño que corresponde, sin animar.
        SetLayersScale(TargetScale);
    }

    private float TargetScale => IsOpen ? 1f : closedScale;

    private void Update()
    {
        // Las tarjetas se agrandan al abrir el tablero y se achican al cerrarlo.
        float current = cardsLayer.localScale.x;
        float target = TargetScale;
        if (Mathf.Approximately(current, target)) return;
        SetLayersScale(Mathf.MoveTowards(current, target, scaleSpeed * Time.deltaTime));
    }

    // Se escalan las dos capas juntas (hacia closedFocus) para que los hilos sigan
    // alineados con las tarjetas y las posiciones guardadas no cambien.
    private void SetLayersScale(float scale)
    {
        ScaleLayer(cardsLayer, _cardsLayerPosition, scale);
        ScaleLayer(threadsLayer, _threadsLayerPosition, scale);
    }

    // Escalar desde un punto que no es el pivot = escalar y correr la capa para que
    // ese punto quede quieto.
    private void ScaleLayer(RectTransform layer, Vector3 basePosition, float scale)
    {
        Rect area = layer.rect;
        Vector3 focus = new Vector3(area.xMin + closedFocus.x * area.width, area.yMin + closedFocus.y * area.height, 0f);
        layer.localScale = new Vector3(scale, scale, 1f);
        layer.localPosition = basePosition + layer.localRotation * (focus * (1f - scale));
    }

    #region Abrir / cerrar

    public void Open(bool editable)
    {
        if (IsOpen) return;

        IsOpen = true;
        Editable = editable;
        boardCamera.SetActive(true);
        controls.SetActive(true);
        editControls.SetActive(editable);
        closeButton.gameObject.SetActive(!editable);
        SuspectData current = FindSuspect(GameProgress.LastVisitedMemory);
        closeSuspectButton.gameObject.SetActive(editable && current != null && !GameProgress.IsClosed(current.Id));
        SetHidden(true);

        SelectColor(ThreadColor.Contradice);

        GameEvents.RaisePlayerLockChanged(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        // Las tarjetas se quedan en el corcho; solo se deja de "usar" el tablero.
        Select(null);
        IsOpen = false;
        Editable = false;
        boardCamera.SetActive(false);
        controls.SetActive(false);
        SetHidden(false);

        GameEvents.RaisePlayerLockChanged(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetHidden(bool hidden)
    {
        foreach (Renderer r in hideWhileOpen)
        {
            if (r != null) r.enabled = !hidden;
        }
    }

    #endregion

    #region Armado

    private void Build()
    {
        Clear();

        // Declaraciones a la izquierda, pistas a la derecha (si el jugador no las movio antes).
        int row = 0;
        foreach (SuspectData suspect in suspects)
        {
            if (!ShowsSuspect(suspect)) continue;
            foreach (StatementData statement in suspect.Statements)
            {
                string text = "<b>" + suspect.DisplayName + "</b>\n\"" + statement.Text + "\"";
                CreateCard(statement, text, statementCardColor, -1, row++);
            }
        }

        row = 0;
        foreach (ClueData clue in GameProgress.FoundClues)
        {
            CreateCard(clue, clue.Text, clueCardColor, 1, row++);
        }

        RefreshThreads();
    }

    private bool ShowsSuspect(SuspectData suspect)
    {
        if (GameProgress.IsVisited(suspect.Id)) return true;
        foreach (ClueData clue in GameProgress.FoundClues)
        {
            if (clue.SuspectId == suspect.Id) return true;
        }
        return false;
    }

    private void CreateCard(ScriptableObject data, string text, Color color, int column, int row)
    {
        BoardCard card = Instantiate(cardPrefab, cardsLayer);
        card.Init(this, data, text, color, selectedCardColor);
        card.Rect.anchorMin = card.Rect.anchorMax = new Vector2(0.5f, 0.5f);

        if (!GameProgress.TryGetCardPosition(data, out Vector2 position))
        {
            Rect area = cardsLayer.rect;
            position = new Vector2(column * area.width * 0.25f, area.height * 0.3f - row * rowSpacing);
        }
        card.Rect.anchoredPosition = position;

        _cards[data] = card;
    }

    private void RefreshThreads()
    {
        foreach (BoardThread thread in _threads) Destroy(thread.gameObject);
        _threads.Clear();

        foreach (Connection connection in GameProgress.Connections)
        {
            if (!_cards.TryGetValue(connection.Clue, out BoardCard clueCard)) continue;
            if (!_cards.TryGetValue(connection.Statement, out BoardCard statementCard)) continue;

            BoardThread thread = Instantiate(threadPrefab, threadsLayer);
            Color color = connection.Color == ThreadColor.Contradice ? redThreadColor : blueThreadColor;
            thread.Init(this, clueCard, statementCard, color);
            _threads.Add(thread);
        }
    }

    private void Clear()
    {
        foreach (BoardCard card in _cards.Values) Destroy(card.gameObject);
        _cards.Clear();
        foreach (BoardThread thread in _threads) Destroy(thread.gameObject);
        _threads.Clear();
        _selected = null;
    }

    #endregion

    #region Hilos

    private void SelectColor(ThreadColor color)
    {
        _color = color;
        // El boton del color elegido se ve un poco mas grande.
        redButton.transform.localScale = Vector3.one * (color == ThreadColor.Contradice ? 1.15f : 1f);
        blueButton.transform.localScale = Vector3.one * (color == ThreadColor.Respalda ? 1.15f : 1f);
    }

    // Click en una tarjeta: la primera queda elegida; la segunda (del otro tipo) cierra el hilo.
    public void OnCardClicked(BoardCard card)
    {
        if (_selected == null)
        {
            Select(card);
            return;
        }

        if (_selected == card)
        {
            Select(null);
            return;
        }

        // Dos pistas o dos declaraciones: no se conectan, se cambia la eleccion.
        if (_selected.IsClue == card.IsClue)
        {
            Select(card);
            return;
        }

        ClueData clue = _selected.IsClue ? _selected.Clue : card.Clue;
        StatementData statement = _selected.IsClue ? card.Statement : _selected.Statement;

        // Mismo hilo otra vez = sacarlo. Otro color = reemplazarlo.
        Connection existing = GameProgress.FindConnection(clue, statement);
        if (existing != null && existing.Color == _color) GameProgress.RemoveConnection(clue, statement);
        else GameProgress.SetConnection(clue, statement, _color);

        Select(null);
        RefreshThreads();
    }

    public void RemoveThread(ClueData clue, StatementData statement)
    {
        GameProgress.RemoveConnection(clue, statement);
        RefreshThreads();
    }

    private void Select(BoardCard card)
    {
        if (_selected != null) _selected.SetSelected(false);
        _selected = card;
        if (_selected != null) _selected.SetSelected(true);
    }

    #endregion

    #region Fin del dia

    // "Finalizar investigacion por hoy": termina el dia y se arranca el siguiente en el
    // despacho. El caso sigue abierto (su documento sigue ahi para volver a entrar).
    private void FinishDay()
    {
        LeaveBoard();
        SceneAdministrator.LoadWithLoadingScreen(SceneName.Despacho);
    }

    // "Cerrar sospechoso": se cierra el caso del recuerdo recien visitado (su documento
    // ya no aparece). Con al menos ContradictionsToUnlock hilos rojos sobre sus
    // declaraciones se ve el fragmento (la noche desde sus ojos); si no, al despacho.
    private void CloseSuspect()
    {
        SuspectData suspect = FindSuspect(GameProgress.LastVisitedMemory);
        if (suspect == null) return;

        LeaveBoard();
        GameProgress.CloseSuspect(suspect.Id);

        bool unlocked = suspect.HasFragment && CountContradictions(suspect) >= suspect.ContradictionsToUnlock;
        if (unlocked) FragmentPlayer.Play(suspect.FragmentScene, suspect.AfterFragmentScene, suspect.FragmentScript);
        else SceneAdministrator.LoadWithLoadingScreen(SceneName.Despacho);
    }

    // Los botones se esconden ya (la carga tarda unos frames), pero el jugador
    // sigue bloqueado hasta que cambie la escena.
    private void LeaveBoard()
    {
        GameProgress.EndOfDayPending = false;
        Select(null);
        Editable = false;
        controls.SetActive(false);
    }

    // Hilos rojos (Contradice) que llegan a alguna declaracion del sospechoso.
    private static int CountContradictions(SuspectData suspect)
    {
        int count = 0;
        foreach (Connection connection in GameProgress.Connections)
        {
            if (connection.Color != ThreadColor.Contradice) continue;
            if (System.Array.IndexOf(suspect.Statements, connection.Statement) >= 0) count++;
        }
        return count;
    }

    // Copia el boton de Finalizar (mismo estilo, y el layout del panel lo acomoda al lado).
    private Button CreateCloseSuspectButton()
    {
        Button button = Instantiate(finishButton, finishButton.transform.parent);
        button.name = "CloseSuspectButton";
        button.onClick.RemoveAllListeners();
        button.transform.SetSiblingIndex(finishButton.transform.GetSiblingIndex() + 1);
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = closeSuspectLabel;
        return button;
    }

    private SuspectData FindSuspect(string id)
    {
        foreach (SuspectData suspect in suspects)
        {
            if (suspect.Id == id) return suspect;
        }
        return null;
    }

    #endregion
}
