using UnityEditor;
using UnityEngine;

// El Render Pipeline Converter de URP solo convierte materiales Built-in -> URP.
// Algunos assets (ej. Survival Props) vienen hechos para HDRP: el converter
// los ignora y se ven rosas. Esto los pasa a "Universal Render Pipeline/Lit"
// copiando las texturas a las propiedades equivalentes de URP.
//
// Como HDRP no esta instalado, el shader original "no existe" y la API de
// Material no ve sus propiedades. Por eso se leen los valores guardados en el
// archivo del material (SerializedObject -> m_SavedProperties).
//
// Uso: seleccionar los materiales (o la carpeta que los contiene) en el
// Project y ejecutar Tools > Convertir materiales HDRP a URP.
public static class ConvertirMaterialesHDRP
{
    [MenuItem("Tools/Convertir materiales HDRP a URP")]
    public static void Convertir()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        int convertidos = 0;

        foreach (string guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string[] materiales = AssetDatabase.IsValidFolder(path)
                ? AssetDatabase.FindAssets("t:Material", new[] { path })
                : new[] { guid };

            foreach (string matGuid in materiales)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(matGuid));
                if (mat == null) continue;
                // Solo los rosas (shader faltante). Los que ya andan no se tocan, aunque
                // tengan restos de propiedades de HDRP guardadas de un shader anterior.
                if (mat.shader != null && mat.shader.name != "Hidden/InternalErrorShader") continue;

                var guardado = new SerializedObject(mat).FindProperty("m_SavedProperties");
                if (BuscarEntrada(guardado, "m_TexEnvs", "_BaseColorMap") == null) continue; // no es de HDRP

                ConvertirMaterial(mat, guardado, urpLit);
                convertidos++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Materiales HDRP convertidos a URP: " + convertidos);
    }

    private static void ConvertirMaterial(Material mat, SerializedProperty guardado, Shader urpLit)
    {
        // Hay que leer todo ANTES de cambiar el shader.
        Texture baseMap = BuscarTextura(guardado, "_BaseColorMap", out Vector2 tiling, out Vector2 offset);
        Texture normalMap = BuscarTextura(guardado, "_NormalMap", out _, out _);
        Texture maskMap = BuscarTextura(guardado, "_MaskMap", out _, out _);
        Texture emissionMap = BuscarTextura(guardado, "_EmissiveColorMap", out _, out _);
        Color baseColor = BuscarColor(guardado, "_BaseColor", Color.white);
        float metallic = BuscarFloat(guardado, "_Metallic", 0f);
        float smoothness = BuscarFloat(guardado, "_Smoothness", 0.5f);
        float normalScale = BuscarFloat(guardado, "_NormalScale", 1f);

        Undo.RecordObject(mat, "Convertir material HDRP a URP");
        mat.shader = urpLit;

        mat.SetTexture("_BaseMap", baseMap);
        mat.SetColor("_BaseColor", baseColor);
        mat.SetTextureScale("_BaseMap", tiling);
        mat.SetTextureOffset("_BaseMap", offset);

        if (normalMap != null)
        {
            mat.SetTexture("_BumpMap", normalMap);
            mat.SetFloat("_BumpScale", normalScale);
            mat.EnableKeyword("_NORMALMAP");
        }

        // El Mask Map de HDRP es R = metalico, G = oclusion, B = detalle, A = suavidad.
        // URP lee el metalico del canal R y la suavidad del alpha del Metallic Map,
        // y la oclusion del canal G del Occlusion Map: la misma textura sirve para los dos.
        if (maskMap != null)
        {
            mat.SetTexture("_MetallicGlossMap", maskMap);
            mat.SetTexture("_OcclusionMap", maskMap);
            mat.SetFloat("_SmoothnessTextureChannel", 0); // 0 = alpha del Metallic Map
            mat.SetFloat("_Smoothness", 1f); // con mapa, este valor multiplica al alpha
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            mat.EnableKeyword("_OCCLUSIONMAP");
        }
        else
        {
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
        }

        if (emissionMap != null)
        {
            mat.SetTexture("_EmissionMap", emissionMap);
            mat.SetColor("_EmissionColor", Color.white);
            mat.EnableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(mat);
    }

    #region Lectura de m_SavedProperties
    // Cada lista (m_TexEnvs, m_Floats, m_Colors) es una lista de pares nombre -> valor.
    private static SerializedProperty BuscarEntrada(SerializedProperty guardado, string lista, string nombre)
    {
        SerializedProperty entradas = guardado.FindPropertyRelative(lista);
        for (int i = 0; i < entradas.arraySize; i++)
        {
            SerializedProperty par = entradas.GetArrayElementAtIndex(i);
            if (par.FindPropertyRelative("first").stringValue == nombre) return par.FindPropertyRelative("second");
        }
        return null;
    }

    private static Texture BuscarTextura(SerializedProperty guardado, string nombre, out Vector2 tiling, out Vector2 offset)
    {
        tiling = Vector2.one;
        offset = Vector2.zero;
        SerializedProperty valor = BuscarEntrada(guardado, "m_TexEnvs", nombre);
        if (valor == null) return null;

        tiling = valor.FindPropertyRelative("m_Scale").vector2Value;
        offset = valor.FindPropertyRelative("m_Offset").vector2Value;
        return valor.FindPropertyRelative("m_Texture").objectReferenceValue as Texture;
    }

    private static float BuscarFloat(SerializedProperty guardado, string nombre, float porDefecto)
    {
        SerializedProperty valor = BuscarEntrada(guardado, "m_Floats", nombre);
        return valor != null ? valor.floatValue : porDefecto;
    }

    private static Color BuscarColor(SerializedProperty guardado, string nombre, Color porDefecto)
    {
        SerializedProperty valor = BuscarEntrada(guardado, "m_Colors", nombre);
        return valor != null ? valor.colorValue : porDefecto;
    }
    #endregion
}
