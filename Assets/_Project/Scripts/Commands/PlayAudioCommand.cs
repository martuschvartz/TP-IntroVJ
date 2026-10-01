using System.Collections;
using UnityEngine;

// Reproduce un audio. Si waitToFinish es true, la cola espera a que termine;
// si es false, sigue enseguida (sirve para mostrar un subtitulo mientras suena).
public class PlayAudioCommand : ICommand
{
    private readonly AudioSource _source;
    private readonly AudioClip _clip;
    private readonly bool _waitToFinish;

    public PlayAudioCommand(AudioSource source, AudioClip clip, bool waitToFinish = true)
    {
        _source = source;
        _clip = clip;
        _waitToFinish = waitToFinish;
    }

    public IEnumerator Execute()
    {
        _source.PlayOneShot(_clip);
        if (_waitToFinish) yield return new WaitForSeconds(_clip.length);
    }
}
