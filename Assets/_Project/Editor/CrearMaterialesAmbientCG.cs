using System.IO;
using UnityEditor;
using UnityEngine;

public static class CrearMaterialesAmbientCG
{
    const string TexturesPath = "Assets/_Project/Art/Textures";
    const string MaterialsPath = "Assets/_Project/Materials";

    [MenuItem("Tools/Crear materiales ambientCG")]
    public static void CrearMateriales()
    {
        int creados = 0;

        foreach (string carpeta in AssetDatabase.GetSubFolders(TexturesPath))
        {
            Texture2D color = BuscarTextura(carpeta, "_Color");
            if (color == null) continue; // no es una carpeta de ambientCG

            Texture2D normal = BuscarTextura(carpeta, "_NormalGL");
            Texture2D ao = BuscarTextura(carpeta, "_AmbientOcclusion");
            Texture2D metalness = BuscarTextura(carpeta, "_Metalness");
            Texture2D emission = BuscarTextura(carpeta, "_Emission");

            if (normal != null)
            {
                MarcarComoNormalMap(normal);
                normal = BuscarTextura(carpeta, "_NormalGL"); // recargar tras el reimport
            }

            string nombre = Path.GetFileName(carpeta);
            string matPath = MaterialsPath + "/" + nombre + ".mat";

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            mat.SetTexture("_BaseMap", color);
            mat.SetFloat("_Smoothness", 0.5f);

            mat.SetTexture("_BumpMap", normal);
            if (normal != null) mat.EnableKeyword("_NORMALMAP");
            else mat.DisableKeyword("_NORMALMAP");

            mat.SetTexture("_OcclusionMap", ao);
            if (ao != null) mat.EnableKeyword("_OCCLUSIONMAP");
            else mat.DisableKeyword("_OCCLUSIONMAP");

            // el JPG no tiene alfa, así que la smoothness queda en 1 * _Smoothness = 0.5
            mat.SetTexture("_MetallicGlossMap", metalness);
            if (metalness != null) mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            else mat.DisableKeyword("_METALLICSPECGLOSSMAP");

            mat.SetTexture("_EmissionMap", emission);
            if (emission != null)
            {
                mat.SetColor("_EmissionColor", Color.white);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }

            EditorUtility.SetDirty(mat);
            creados++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Materiales ambientCG creados/actualizados: " + creados);
    }

    static Texture2D BuscarTextura(string carpeta, string sufijo)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { carpeta }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path).EndsWith(sufijo))
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        return null;
    }

    static void MarcarComoNormalMap(Texture2D textura)
    {
        string path = AssetDatabase.GetAssetPath(textura);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType == TextureImporterType.NormalMap) return;

        importer.textureType = TextureImporterType.NormalMap;
        importer.SaveAndReimport();
    }
}
