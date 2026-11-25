using System.Collections;
using UnityEngine;

// Va en el padre de las barras ("barras carcel"), que tiene el Rigidbody.
// Los colliders de las barras hijas se suman en un solo collider compuesto,
// asi que la reja cae entera, como un porton.
// Arranca colgada (kinematic) y a los "delay" segundos se suelta: desde ahi la
// mueve la gravedad. Al golpear el piso suena y levanta polvo (los dos opcionales).
[RequireComponent(typeof(Rigidbody))]
public class JailGateDrop : MonoBehaviour
{
    [Tooltip("Segundos antes de soltar la reja.")]
    [SerializeField] private float delay = 1.5f;

    [Header("Golpe contra el piso (opcional)")]
    [SerializeField] private AudioSource impactSound;
    [SerializeField] private ParticleSystem dust;

    private Rigidbody _rb;
    private bool _landed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true; // colgada hasta que se suelte
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        _rb.isKinematic = false; // desde aca la mueve la gravedad
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Solo el primer golpe: los rebotes no repiten sonido ni polvo.
        if (_landed) return;
        _landed = true;

        if (impactSound != null) impactSound.Play();

        if (dust != null)
        {
            dust.transform.position = collision.GetContact(0).point;
            dust.Play();
        }
    }
}
