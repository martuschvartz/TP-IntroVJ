using System.Collections;
using UnityEngine;

// Final de "No confesar" (escena "No carcel"): la reja arranca apoyada en el piso
// y se levanta, como si abrieran la celda y los liberaran.
// Va en el padre de las barras ("barras carcel"). Si tiene Rigidbody lo deja
// kinematic y lo mueve con MovePosition, asi no lo empuja la gravedad.
public class JailGateRise : MonoBehaviour
{
    [Tooltip("Segundos antes de empezar a subir.")]
    [SerializeField] private float delay = 1.5f;

    [Tooltip("Cuanto sube la reja (unidades del mundo).")]
    [SerializeField] private float riseHeight = 400f;

    [Tooltip("Segundos que tarda en subir.")]
    [SerializeField] private float duration = 4f;

    [Header("Traba (antes de subir)")]
    [Tooltip("Segundos que la reja tiembla, como si se destrabara.")]
    [SerializeField] private float rattleDuration = 0.6f;
    [SerializeField] private float rattleAmount = 1.5f;

    [Header("Opcional")]
    [SerializeField] private AudioSource unlockSound;
    [SerializeField] private AudioSource liftSound;

    private Rigidbody _rb;
    private Vector3 _start;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
        _start = transform.position;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);

        // Destrabe: un temblor corto.
        if (unlockSound != null) unlockSound.Play();
        for (float t = 0f; t < rattleDuration; t += Time.deltaTime)
        {
            float a = rattleAmount * (1f - t / rattleDuration);
            SetPosition(_start + new Vector3(0f, Random.Range(-a, a) * 0.5f + a * 0.5f, 0f));
            yield return null;
        }
        SetPosition(_start);

        // Subida: arranca lento (pesa), acelera y frena al final.
        if (liftSound != null) liftSound.Play();
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            SetPosition(_start + Vector3.up * (riseHeight * k));
            yield return null;
        }
        SetPosition(_start + Vector3.up * riseHeight);
        if (liftSound != null) liftSound.Stop();
    }

    private void SetPosition(Vector3 p)
    {
        if (_rb != null) _rb.MovePosition(p);
        else transform.position = p;
    }
}
