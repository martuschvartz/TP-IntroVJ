using UnityEngine;
using UnityEngine.InputSystem;

// Atajo de prueba: en el despacho, reproduce el fragmento de un sospechoso sin tener
// que resolver su tablero. F9 = el primero de la lista, F10 = el segundo, F11 = el tercero.
// Usa los mismos datos que el tablero (fragmentScene, afterFragmentScene, fragmentScript),
// asi que prueba exactamente lo que se va a ver en el juego.
//
// Solo anda en el editor o en una Development Build: en el juego final no hace nada.
// Uso: ponerlo en cualquier objeto del Despacho y cargar los SuspectData en la lista.
public class FragmentShortcut : MonoBehaviour
{
    [Tooltip("F9 = elemento 0, F10 = elemento 1, F11 = elemento 2.")]
    [SerializeField] private SuspectData[] suspects;

    private void Update()
    {
        if (!Debug.isDebugBuild) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || suspects == null) return;

        if (keyboard.f9Key.wasPressedThisFrame) PlayFragment(0);
        else if (keyboard.f10Key.wasPressedThisFrame) PlayFragment(1);
        else if (keyboard.f11Key.wasPressedThisFrame) PlayFragment(2);
    }

    private void PlayFragment(int index)
    {
        if (index >= suspects.Length || suspects[index] == null)
        {
            Debug.LogWarning("[FragmentShortcut] No hay sospechoso cargado en el lugar " + index + ".", this);
            return;
        }

        SuspectData suspect = suspects[index];
        if (!suspect.HasFragment)
        {
            Debug.LogWarning("[FragmentShortcut] " + suspect.DisplayName + " no tiene fragmento (hasFragment apagado).", this);
            return;
        }

        Debug.Log("[FragmentShortcut] Fragmento de " + suspect.DisplayName + ": " + suspect.FragmentScene);
        enabled = false; // que no se dispare dos veces mientras carga
        FragmentPlayer.Play(suspect.FragmentScene, suspect.AfterFragmentScene, suspect.FragmentScript);
    }
}
