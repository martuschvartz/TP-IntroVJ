using System.Collections.Generic;
using UnityEngine;

// Prende / apaga el brillo de una pista: le suma a cada Renderer que le pasan
// un material extra con el shader InteractableGlow,
// y al apagarlo le devuelve sus materiales originales. Lo usa PlayerInteractor.
// Usa sharedMaterials para no crear copias de los materiales del objeto.
public class InteractableGlow
{
    private const string ShaderName = "InteractableGlow"; // Resources/InteractableGlow.shader

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int PulseSpeedId = Shader.PropertyToID("_PulseSpeed");

    private readonly Material _material;
    private readonly Dictionary<Renderer, Material[]> _originals = new Dictionary<Renderer, Material[]>();
    private object _target;

    public InteractableGlow(Color color, float outlineWidth, float pulseSpeed)
    {
        Shader shader = Resources.Load<Shader>(ShaderName);
        if (shader == null)
        {
            Debug.LogWarning("[InteractableGlow] No se encontro Resources/" + ShaderName + ".shader: no hay brillo.");
            return;
        }

        _material = new Material(shader) { name = "InteractableGlow (runtime)" };
        _material.SetColor(ColorId, color);
        _material.SetFloat(OutlineWidthId, outlineWidth);
        _material.SetFloat(PulseSpeedId, pulseSpeed);
    }

    // target = quien brilla (para no rehacerlo cada frame); null = no brilla nada.
    public void SetTarget(object target, IReadOnlyList<Renderer> renderers)
    {
        if (target == _target) return;
        Clear();
        _target = target;
        if (_material == null || target == null || renderers == null) return;

        foreach (Renderer r in renderers)
        {
            if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
            if (_originals.ContainsKey(r)) continue;

            Material[] original = r.sharedMaterials;
            Material[] withGlow = new Material[original.Length + 1];
            original.CopyTo(withGlow, 0);
            withGlow[original.Length] = _material;
            _originals[r] = original;
            r.sharedMaterials = withGlow;
        }
    }

    private void Clear()
    {
        foreach (KeyValuePair<Renderer, Material[]> pair in _originals)
        {
            // El objeto pudo haberse destruido (ej: destroyOnReveal).
            if (pair.Key != null) pair.Key.sharedMaterials = pair.Value;
        }
        _originals.Clear();
        _target = null;
    }
}
