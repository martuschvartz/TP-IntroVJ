using System.Collections.Generic;
using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

// El tablero de investigacion. Las tarjetas y los hilos viven en un canvas World Space
// apoyado sobre el corcho 3D del despacho, asi se ven siempre (aunque no lo estes usando).
// "Abrir" el tablero = prender una CinemachineCamera que mira al corcho (Cinemachine hace
// la transicion), liberar el cursor y mostrar los botones.
// - El corcho esta dividido en zonas, una por sospechoso (BoardZone). Al abrir, la camara
//   se acerca a una zona; "Ver todo" se aleja para ver el corcho entero.
// - Siempre se puede editar (mover tarjetas, tirar y sacar hilos) mientras el caso
//   este abierto: el tablero es un borrador. La X sale sin cerrar nada.
// - "Cerrar sospechoso" cierra el caso de la zona que se esta mirando: sus tarjetas e hilos
//   quedan fijos y, si esta bien resuelto, se ve su fragmento.
// - Se abre solo al volver de un recuerdo, acercado a la zona de ese sospechoso.
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
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [Tooltip("Cierra el caso de la zona que se esta mirando. Solo aparece acercado a una zona con el caso abierto.")]
    [SerializeField] private Button closeSuspectButton;
    [Tooltip("La X: sale del tablero sin cerrar nada.")]
    [SerializeField] private Button closeButton;

    [Header("Datos")]
    [Tooltip("Todos los sospechosos. Se muestran las declaraciones de los que ya se visitaron. " +
             "Los que tienen zona se suman solos aunque no esten en esta lista.")]
    [SerializeField] private SuspectData[] suspects;

    [Header("Colores")]
    [SerializeField] private Color clueCardColor = new Color(0.96f, 0.94f, 0.88f);
    [SerializeField] private Color statementCardColor = new Color(1f, 0.93f, 0.6f);
    [SerializeField] private Color selectedCardColor = new Color(0.6f, 0.85f, 1f);
    [SerializeField] private Color redThreadColor = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color blueThreadColor = new Color(0.15f, 0.35f, 0.85f);
    [Tooltip("Cuanto se oscurecen las tarjetas de un caso cerrado (0 = nada).")]
    [Range(0f, 1f)]
    [SerializeField] private float closedCardDarken = 0.3f;

    [Header("Ubicacion inicial de las tarjetas")]
    [Tooltip("Solo para tarjetas de un sospechoso sin zona.")]
    [SerializeField] private float rowSpacing = 180f;
    [Tooltip("Espacio entre tarjetas dentro de una zona, en proporcion al tamaño de la tarjeta (1.1 = 10% de aire).")]
    [SerializeField] private float cardSpacing = 1.1f;

    [Header("Zonas (una por sospechoso)")]
    [Tooltip("Las zonas del corcho. Vacio = se buscan solas entre los hijos del canvas.")]
    [SerializeField] private BoardZone[] zones;
    [Tooltip("Boton para alejarse y ver el corcho entero. Solo aparece acercado a una zona.")]
    [SerializeField] private Button overviewButton;

    private readonly Dictionary<ScriptableObject, BoardCard> _cards = new Dictionary<ScriptableObject, BoardCard>();
    private readonly List<BoardThread> _threads = new List<BoardThread>();
    private BoardCard _selected;
    private BoardZone _focused;
    private ThreadColor _color = ThreadColor.Contradice;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        boardCamera.SetActive(false);
        controls.SetActive(false);
        if (zones == null || zones.Length == 0) zones = GetComponentsInChildren<BoardZone>(true);
        foreach (BoardZone zone in zones)
        {
            zone.Init(this);
            // Por si quedo prendida en la escena: con prioridad alta le robaria la vista al jugador.
            if (zone.ZoneCamera != null) zone.ZoneCamera.SetActive(false);
        }
        // Los dos se arman en la UI (no se crean por codigo). Si falta alguno se avisa y el resto anda igual.
        if (overviewButton != null)
        {
            overviewButton.onClick.AddListener(() => FocusZone(null));
            overviewButton.gameObject.SetActive(false);
        }
        else Debug.LogWarning("[BoardUI] Falta asignar Overview Button (Ver todo).", this);
        redButton.onClick.AddListener(() => SelectColor(ThreadColor.Contradice));
        blueButton.onClick.AddListener(() => SelectColor(ThreadColor.Respalda));
        if (closeSuspectButton != null) closeSuspectButton.onClick.AddListener(CloseSuspect);
        else Debug.LogWarning("[BoardUI] Falta asignar Close Suspect Button (Cerrar sospechoso).", this);
        closeButton.onClick.AddListener(Close);
    }

    private void Start()
    {
        // Que los tamaños de la UI (zonas, layouts) ya esten calculados antes de ubicar tarjetas.
        Canvas.ForceUpdateCanvases();

        // El corcho muestra siempre lo que se fue armando.
        Build();

        // Volvimos de un recuerdo: se abre acercado a la zona de ese sospechoso para conectar
        // lo nuevo (se puede salir con la X y hacerlo despues).
        if (GameProgress.EndOfDayPending)
        {
            GameProgress.EndOfDayPending = false;
            Open();
            FocusZone(FindZone(GameProgress.LastVisitedMemory));
        }
    }

    #region Abrir / cerrar

    // Desde el corcho 3D: se acerca a la zona donde estaba mirando el jugador.
    public void OpenAt(Vector3 worldPoint)
    {
        Open();
        FocusZone(FindZoneAt(worldPoint));
    }

    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;
        boardCamera.SetActive(true);
        controls.SetActive(true);
        RefreshZoneButtons();
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
        FocusZone(null);
        IsOpen = false;
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

        // Cada sospechoso en su zona: declaraciones en la primera columna, pistas a la derecha.
        // Si el jugador ya movio una tarjeta, se respeta donde la dejo.
        // Lo que no tiene zona va como antes: declaraciones a la izquierda y pistas a la derecha del corcho.
        int looseStatementRow = 0;
        foreach (SuspectData suspect in AllSuspects())
        {
            if (!ShowsSuspect(suspect)) continue;
            BoardZone zone = FindZone(suspect.Id);
            int index = 0;
            foreach (StatementData statement in suspect.Statements)
            {
                // En su zona no hace falta el nombre (la zona ya dice de quien es); sin zona, si.
                string quote = "\"" + statement.Text + "\"";
                string text = zone != null ? quote : "<b>" + suspect.DisplayName + "</b>\n" + quote;
                Vector2 position = zone != null
                    ? ZoneSlot(zone, 0, index++)
                    : LooseSlot(-1, looseStatementRow++);
                CreateCard(statement, text, statementCardColor, position);
            }
        }

        int looseClueRow = 0;
        Dictionary<BoardZone, int> cluesPerZone = new Dictionary<BoardZone, int>();
        foreach (ClueData clue in GameProgress.FoundClues)
        {
            BoardZone zone = FindZone(clue.SuspectId);
            Vector2 position;
            if (zone != null)
            {
                cluesPerZone.TryGetValue(zone, out int index);
                cluesPerZone[zone] = index + 1;
                position = ZoneClueSlot(zone, index);
            }
            else position = LooseSlot(1, looseClueRow++);
            CreateCard(clue, clue.Text, clueCardColor, position);
        }

        RefreshThreads();
    }

    // Tamaño de una tarjeta (con aire) en coordenadas del corcho.
    private Vector2 CardStep => ((RectTransform)cardPrefab.transform).rect.size * cardSpacing;

    // Lugar en la grilla de una zona: columna 0 = declaraciones, 1 en adelante = pistas.
    // Se llena de arriba hacia abajo, en el espacio de la capa de tarjetas.
    private Vector2 ZoneSlot(BoardZone zone, int column, int row)
    {
        Rect area = ZoneRectInCardsLayer(zone);
        Vector2 step = CardStep;
        Vector2 local = new Vector2(
            area.xMin + step.x * (column + 0.5f),
            area.yMax - step.y * (row + 0.5f));
        // anchoredPosition se mide desde el centro de la capa (las tarjetas tienen el ancla al medio).
        return local - cardsLayer.rect.center;
    }

    // Las pistas llenan columnas a la derecha de las declaraciones; cuando una se llena, sigue la proxima.
    private Vector2 ZoneClueSlot(BoardZone zone, int index)
    {
        int rowsPerColumn = Mathf.Max(1, Mathf.FloorToInt(ZoneRectInCardsLayer(zone).height / CardStep.y));
        return ZoneSlot(zone, 1 + index / rowsPerColumn, index % rowsPerColumn);
    }

    private Vector2 LooseSlot(int column, int row)
    {
        Rect area = cardsLayer.rect;
        return new Vector2(column * area.width * 0.25f, area.height * 0.3f - row * rowSpacing);
    }

    // El rectangulo de la zona, pasado a coordenadas de la capa de tarjetas
    // (la zona puede estar en otro lugar de la jerarquia del canvas).
    private Rect ZoneRectInCardsLayer(BoardZone zone)
    {
        Vector3[] corners = new Vector3[4];
        zone.Rect.GetWorldCorners(corners);
        Vector2 min = cardsLayer.InverseTransformPoint(corners[0]);
        Vector2 max = cardsLayer.InverseTransformPoint(corners[2]);
        return Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
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

    private void CreateCard(ScriptableObject data, string text, Color color, Vector2 defaultPosition)
    {
        // Caso cerrado: la tarjeta se ve mas apagada (y no se puede tocar, ver CanEdit).
        if (IsClosedSuspect(SuspectIdOf(data))) color = Color.Lerp(color, Color.black, closedCardDarken);

        BoardCard card = Instantiate(cardPrefab, cardsLayer);
        card.Init(this, data, text, color, selectedCardColor);
        card.Rect.anchorMin = card.Rect.anchorMax = new Vector2(0.5f, 0.5f);

        if (!GameProgress.TryGetCardPosition(data, out Vector2 position)) position = defaultPosition;
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

    // Se puede tocar una tarjeta (moverla, conectarla, sacarle hilos) si el tablero esta abierto,
    // acercado a SU zona (la zona activa) y el caso de su sospechoso no se cerro.
    // En "Ver todo" no se toca nada: es solo para mirar. Un hilo se puede tocar si sus dos puntas se pueden.
    public bool CanEdit(BoardCard card)
    {
        return card != null && IsOpen && _focused != null
               && FindZone(card.SuspectId) == _focused
               && !IsClosedSuspect(card.SuspectId);
    }

    // Donde se puede arrastrar una tarjeta, en coordenadas de la capa de tarjetas:
    // la zona de su sospechoso, o el corcho entero si no tiene zona.
    public Rect DragArea(BoardCard card)
    {
        BoardZone zone = card != null ? FindZone(card.SuspectId) : null;
        return zone != null ? ZoneRectInCardsLayer(zone) : cardsLayer.rect;
    }

    private static bool IsClosedSuspect(string suspectId)
    {
        return !string.IsNullOrEmpty(suspectId) && GameProgress.IsClosed(suspectId);
    }

    private static string SuspectIdOf(ScriptableObject data)
    {
        if (data is ClueData clue) return clue.SuspectId;
        if (data is StatementData statement) return statement.SuspectId;
        return null;
    }

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

    #region Cerrar sospechoso

    // "Cerrar sospechoso": se cierra el caso de la zona que se esta mirando (su documento
    // ya no aparece y sus tarjetas quedan fijas). Si su tablero esta bien resuelto (IsSolved)
    // se ve el fragmento (la noche desde sus ojos); si no, se vuelve al despacho.
    private void CloseSuspect()
    {
        SuspectData suspect = ClosableSuspect();
        if (suspect == null) return;

        LeaveBoard();
        GameProgress.CloseSuspect(suspect.Id);

        bool unlocked = suspect.HasFragment && IsSolved(suspect);
        if (unlocked) FragmentPlayer.Play(suspect.FragmentScene, suspect.AfterFragmentScene, suspect.FragmentScript);
        else SceneAdministrator.LoadWithLoadingScreen(SceneName.Despacho);
    }

    // Los botones se esconden ya (la carga tarda unos frames), pero el jugador
    // sigue bloqueado hasta que cambie la escena.
    private void LeaveBoard()
    {
        GameProgress.EndOfDayPending = false;
        Select(null);
        IsOpen = false; // que no se pueda tocar nada mientras carga
        controls.SetActive(false);
    }

    // Resuelto = el fragmento es la suma de las inconsistencias:
    //  1) ningun hilo mal puesto en las pistas del sospechoso (a otra declaracion o con otro color), y
    //  2) cada declaracion que es mentira (IsLie) tiene al menos una pista conectada con hilo rojo.
    // Respaldar una verdad con azul esta bien, pero no desbloquea nada. Las pistas sin hilo
    // no cuentan: se puede salir del recuerdo sin encontrar todo.
    // Las pistas ambiguas no cuentan como error con ningun hilo, pero tampoco exponen mentiras.
    private static bool IsSolved(SuspectData suspect)
    {
        foreach (Connection connection in GameProgress.Connections)
        {
            ClueData clue = connection.Clue;
            if (clue == null || clue.SuspectId != suspect.Id || clue.Ambiguous) continue;
            if (connection.Statement != clue.RelatedStatement || connection.Color != clue.ExpectedColor) return false;
        }

        bool anyLie = false;
        foreach (StatementData statement in suspect.Statements)
        {
            if (!statement.IsLie) continue;
            anyLie = true;
            if (!IsExposed(statement)) return false;
        }

        // Un sospechoso sin mentiras no tiene nada que desbloquear.
        return anyLie;
    }

    // La mentira esta expuesta si alguna pista suya (no ambigua) esta conectada con rojo.
    // Como IsSolved ya descarto hilos mal puestos, ese rojo es el color esperado.
    private static bool IsExposed(StatementData statement)
    {
        foreach (Connection connection in GameProgress.Connections)
        {
            if (connection.Statement != statement || connection.Color != ThreadColor.Contradice) continue;
            if (connection.Clue != null && !connection.Clue.Ambiguous) return true;
        }
        return false;
    }

    #endregion

    #region Zonas

    // Acerca la camara a una zona (null = alejarse y ver el corcho entero con BoardCam).
    // Cada zona tiene su camara armada a mano: al prender una, Cinemachine hace la
    // transicion desde la que estaba (misma prioridad = gana la ultima prendida).
    public void FocusZone(BoardZone zone)
    {
        if (zone == _focused) return;
        if (zone != null && !IsOpen) return;

        // Una tarjeta elegida de la zona anterior no puede quedar esperando su segundo click.
        Select(null);

        if (_focused != null && _focused.ZoneCamera != null) _focused.ZoneCamera.SetActive(false);
        _focused = zone;
        if (_focused != null && _focused.ZoneCamera != null) _focused.ZoneCamera.SetActive(true);
        RefreshZoneButtons();
    }

    // Acercado a una zona: botones de hilo, "Ver todo" y (si se puede) "Cerrar sospechoso".
    // En "Ver todo" solo se mira, asi que no hay botones de hilo.
    private void RefreshZoneButtons()
    {
        bool inZone = IsOpen && _focused != null;
        redButton.gameObject.SetActive(inZone);
        blueButton.gameObject.SetActive(inZone);
        if (overviewButton != null) overviewButton.gameObject.SetActive(IsOpen && _focused != null);
        if (closeSuspectButton != null) closeSuspectButton.gameObject.SetActive(IsOpen && ClosableSuspect() != null);
    }

    // El sospechoso de la zona que se esta mirando, si ya se visito y su caso sigue abierto.
    private SuspectData ClosableSuspect()
    {
        if (_focused == null || _focused.Suspect == null) return null;
        SuspectData suspect = _focused.Suspect;
        return ShowsSuspect(suspect) && !GameProgress.IsClosed(suspect.Id) ? suspect : null;
    }

    private BoardZone FindZone(string suspectId)
    {
        if (string.IsNullOrEmpty(suspectId)) return null;
        foreach (BoardZone zone in zones)
        {
            if (zone != null && zone.Suspect != null && zone.Suspect.Id == suspectId) return zone;
        }
        return null;
    }

    private BoardZone FindZoneAt(Vector3 worldPoint)
    {
        foreach (BoardZone zone in zones)
        {
            if (zone != null && zone.Contains(worldPoint)) return zone;
        }
        return null;
    }

    #endregion

    #region Datos

    // Los de la lista Suspects mas los que tienen zona (sin repetir), asi no se olvida ninguno.
    private List<SuspectData> AllSuspects()
    {
        List<SuspectData> all = new List<SuspectData>();
        if (suspects != null)
        {
            foreach (SuspectData suspect in suspects)
            {
                if (suspect != null && !all.Contains(suspect)) all.Add(suspect);
            }
        }
        foreach (BoardZone zone in zones)
        {
            if (zone != null && zone.Suspect != null && !all.Contains(zone.Suspect)) all.Add(zone.Suspect);
        }
        return all;
    }

    #endregion
}
