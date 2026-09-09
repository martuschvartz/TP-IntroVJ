using UnityEngine;

// Cuenta regresiva global. No sabe nada de habitaciones ni de UI: solo
// avisa por evento cuando el tiempo se acaba, y por su cuenta le suma un
// poco de tiempo extra cada vez que se encuentra un objeto clave (sea cual
// sea la zona). Se puede probar solo, sin RoomManager, viendo bajar
// "Remaining" y el log de TimeUp.
public class TimerController : MonoBehaviour
{
    [SerializeField] private float _duration = 10f;
    [SerializeField] private float _bonusOnObjectFound = 10f;

    private float _remaining;
    private bool _isRunning;
    private bool _hasFired;

    public float Remaining => _remaining;

    #region Suscripcion a eventos
    private void OnEnable()
    {
        GameEvents.ObjectFound += HandleObjectFound;
    }

    private void OnDisable()
    {
        GameEvents.ObjectFound -= HandleObjectFound;
    }
    #endregion

    private void Start()
    {
        _remaining = _duration;
        _isRunning = true;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _remaining -= Time.deltaTime;

        if (_remaining <= 0f)
        {
            _remaining = 0f;
            _isRunning = false;

            if (!_hasFired)
            {
                _hasFired = true;
                GameEvents.RaiseTimeUp();
            }
        }
    }

    private void HandleObjectFound(ZoneId zone)
    {
        AddTime(_bonusOnObjectFound);
    }

    public void AddTime(float seconds)
    {
        // Si ya se acabo el tiempo, no tiene sentido seguir sumando.
        if (_hasFired) return;
        _remaining += seconds;
    }

    public void Pause()
    {
        _isRunning = false;
    }
}
