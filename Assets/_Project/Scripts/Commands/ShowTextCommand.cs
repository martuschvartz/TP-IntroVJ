using System.Collections;
using TMPro;
using UnityEngine;

// Muestra un texto en pantalla (subtitulo, fecha...) durante unos segundos y lo borra.
public class ShowTextCommand : ICommand
{
    private readonly TMP_Text _label;
    private readonly string _text;
    private readonly float _seconds;

    public ShowTextCommand(TMP_Text label, string text, float seconds)
    {
        _label = label;
        _text = text;
        _seconds = seconds;
    }

    public IEnumerator Execute()
    {
        _label.text = _text;
        yield return new WaitForSeconds(_seconds);
        _label.text = "";
    }
}
