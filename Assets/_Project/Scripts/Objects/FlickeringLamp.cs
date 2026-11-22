using System.Collections;
using UnityEngine;

namespace _Project.Scripts.Objects
{
    /// <summary>
    /// Hace titilar un farol: largos ratos prendido y, de golpe, una ráfaga de cortes.
    /// Apaga las luces hijas y la emisión del globo. La emisión se cambia con un
    /// MaterialPropertyBlock para no tocar el material compartido con los otros faroles.
    /// </summary>
    public class FlickeringLamp : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Referencias")]
        [Tooltip("Luces del farol (Spot y Point). Si se deja vacío, toma las Light hijas.")]
        [SerializeField] private Light[] lights;

        [Tooltip("Renderer del globo. Si se deja vacío, toma el de este objeto.")]
        [SerializeField] private Renderer lampRenderer;

        [Header("Tiempo prendido entre ráfagas (segundos)")]
        [SerializeField] private float minWaitOn = 2f;
        [SerializeField] private float maxWaitOn = 6f;

        [Header("Ráfaga")]
        [SerializeField] private int minFlickers = 2;
        [SerializeField] private int maxFlickers = 5;
        [SerializeField] private float minOffTime = 0.05f;
        [SerializeField] private float maxOffTime = 0.15f;
        [SerializeField] private float minOnTime = 0.05f;
        [SerializeField] private float maxOnTime = 0.2f;

        [Tooltip("Cuánto queda encendido al 'apagarse' (0 = apagado total).")]
        [Range(0f, 1f)]
        [SerializeField] private float offLevel = 0.05f;

        private float[] baseIntensities;
        private Color baseEmission;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            if (lights == null || lights.Length == 0)
                lights = GetComponentsInChildren<Light>();

            if (lampRenderer == null)
                lampRenderer = GetComponent<Renderer>();

            baseIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
                baseIntensities[i] = lights[i].intensity;

            propertyBlock = new MaterialPropertyBlock();
            if (lampRenderer != null)
                baseEmission = lampRenderer.sharedMaterial.GetColor(EmissionColorId);
        }

        private void OnEnable()
        {
            StartCoroutine(FlickerLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            SetLevel(1f);
        }

        private IEnumerator FlickerLoop()
        {
            while (true)
            {
                SetLevel(1f);
                yield return new WaitForSeconds(Random.Range(minWaitOn, maxWaitOn));

                int flickers = Random.Range(minFlickers, maxFlickers + 1);
                for (int i = 0; i < flickers; i++)
                {
                    SetLevel(offLevel);
                    yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));

                    SetLevel(1f);
                    yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));
                }
            }
        }

        /// <param name="level">1 = como quedó en el Inspector, 0 = apagado.</param>
        private void SetLevel(float level)
        {
            for (int i = 0; i < lights.Length; i++)
                lights[i].intensity = baseIntensities[i] * level;

            if (lampRenderer == null) return;

            lampRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, baseEmission * level);
            lampRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
