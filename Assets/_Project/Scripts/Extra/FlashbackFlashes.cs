using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Poné este script en un GameObject vacío dentro de la escena "room".
// Precarga "room-flashback" de forma aditiva, la deja oculta y la muestra
// durante un instante cada cierto tiempo aleatorio. La cámara del flashback
// copia en todo momento la posición y rotación de la cámara del jugador,
// así el flash se ve desde el mismo lugar y mirando hacia el mismo lado.
public class FlashbackFlashes : MonoBehaviour
{
    [SerializeField] private string flashbackScene = "room-flashback";

    [Header("Cámara del jugador en 'room' (si queda vacío usa Camera.main)")]
    [SerializeField] private Camera playerCamera;

    [Header("Desplazamiento entre room y room-flashback")]
    [Tooltip("Dejalo en 0,0,0 si ambas habitaciones están en el mismo lugar del mundo. " +
             "Si moviste la geometría del flashback (ej. y = 1000), poné ese mismo valor.")]
    [SerializeField] private Vector3 flashbackOffset = Vector3.zero;

    [Header("Tiempo entre flashes (segundos)")]
    [SerializeField] private float minInterval = 8f;
    [SerializeField] private float maxInterval = 20f;

    [Header("Duración del flash (segundos)")]
    [SerializeField] private float flashDuration = 1f;

    private readonly List<GameObject> flashbackRoots = new List<GameObject>();
    private Camera flashbackCamera;

    private IEnumerator Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        // 1) Cargar la escena del flashback una sola vez, sin reemplazar "room"
        AsyncOperation op = SceneManager.LoadSceneAsync(flashbackScene, LoadSceneMode.Additive);
        yield return op;

        // 2) Guardar sus objetos raíz y buscar su cámara
        Scene scene = SceneManager.GetSceneByName(flashbackScene);
        scene.GetRootGameObjects(flashbackRoots);
        foreach (GameObject go in flashbackRoots)
        {
            flashbackCamera = go.GetComponentInChildren<Camera>(true);
            if (flashbackCamera != null) break;
        }
        if (flashbackCamera == null)
            Debug.LogWarning("No encontré una cámara en " + flashbackScene);

        SetFlashbackVisible(false);

        // 3) Bucle de flashes
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            SyncCamera(); // alinear antes de mostrar, para que no se vea un salto
            SetFlashbackVisible(true);
            yield return new WaitForSeconds(flashDuration);
            SetFlashbackVisible(false);
        }
    }

    // LateUpdate corre después de que el jugador se movió en el frame,
    // así la cámara del flashback nunca queda un frame atrasada.
    private void LateUpdate()
    {
        SyncCamera();
    }

    private void SyncCamera()
    {
        if (playerCamera == null || flashbackCamera == null) return;

        flashbackCamera.transform.SetPositionAndRotation(
            playerCamera.transform.position + flashbackOffset,
            playerCamera.transform.rotation);

        // Mismo campo de visión para que el encuadre coincida
        flashbackCamera.fieldOfView = playerCamera.fieldOfView;
    }

    private void SetFlashbackVisible(bool visible)
    {
        foreach (GameObject go in flashbackRoots)
            if (go != null) go.SetActive(visible);
    }

    private void OnDestroy()
    {
        Scene scene = SceneManager.GetSceneByName(flashbackScene);
        if (scene.isLoaded)
            SceneManager.UnloadSceneAsync(scene);
    }
}
