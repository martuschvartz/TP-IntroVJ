using UnityEngine;
using UnityEngine.UI;

// Unica responsabilidad: mostrar el tiempo restante en pantalla.
// Mismo patron que el Crosshair: un Text de Canvas leyendo un valor de otro
// script cada frame. No decide nada del juego, solo lee TimerController.
public class TimerUI : MonoBehaviour
{
    [SerializeField] private TimerController _timer;
    [SerializeField] private Text _label;

    private void Update()
    {
        int secondsLeft = Mathf.CeilToInt(_timer.Remaining);
        _label.text = secondsLeft.ToString();
    }
}
