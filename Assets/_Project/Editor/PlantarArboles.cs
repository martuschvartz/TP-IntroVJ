using UnityEditor;
using UnityEngine;

// Planta arboles a los dos costados de un camino recto, para no ponerlos a mano.
// El camino va de "Inicio" a "Fin" (ej: la pickup y la cabaña). Cada arbol cae a una
// distancia al azar del eje del camino (nunca encima), con rotacion y escala al azar,
// apoyado sobre el Terrain.
//
// Los arboles se crean como instancias del prefab (no copias sueltas): todos comparten
// malla y material (Flyweight). Se marcan Static para que Unity los agrupe al dibujar.
//
// Uso: Tools > Plantar arboles en el camino. Se puede deshacer con Ctrl+Z,
// o volver a plantar con otra Semilla (los anteriores del mismo padre se borran).
public class PlantarArboles : ScriptableWizard
{
    [Tooltip("Donde empieza el camino (ej: la pickup).")]
    public Transform inicio;
    [Tooltip("Donde termina el camino (ej: la cabaña).")]
    public Transform fin;
    [Tooltip("Prefabs de arboles. Se elige uno al azar para cada arbol.")]
    public GameObject[] prefabs;

    [Header("Cantidad y lugar")]
    public int cantidad = 60;
    [Tooltip("Distancia minima al eje del camino: deja libre el camino.")]
    public float distanciaMinima = 450f;
    [Tooltip("Distancia maxima al eje del camino.")]
    public float distanciaMaxima = 1800f;
    [Tooltip("Cuanto se sigue plantando antes del inicio y despues del fin.")]
    public float margenExtremos = 800f;

    [Header("Variacion")]
    public float escalaMinima = 50f;
    public float escalaMaxima = 65f;
    [Tooltip("Misma semilla = mismos arboles. Cambiala para otra distribucion.")]
    public int semilla = 1;

    [Header("Jerarquia")]
    [Tooltip("Objeto que agrupa los arboles. Si existe, se vacia antes de plantar.")]
    public string nombrePadre = "Arboles";

    [MenuItem("Tools/Plantar arboles en el camino")]
    private static void Abrir()
    {
        DisplayWizard<PlantarArboles>("Plantar arboles en el camino", "Plantar");
    }

    private void OnWizardUpdate()
    {
        errorString = "";
        if (inicio == null || fin == null) errorString = "Falta Inicio o Fin.";
        else if (prefabs == null || prefabs.Length == 0) errorString = "Falta al menos un prefab de arbol.";
        else if (distanciaMaxima <= distanciaMinima) errorString = "La distancia maxima tiene que ser mayor que la minima.";
        isValid = errorString == "";
    }

    private void OnWizardCreate()
    {
        Undo.SetCurrentGroupName("Plantar arboles");
        int undoGroup = Undo.GetCurrentGroup();

        Transform padre = PrepararPadre();
        Random.State estadoAnterior = Random.state;
        Random.InitState(semilla);

        // Eje del camino en el plano (sin altura).
        Vector3 a = inicio.position;
        Vector3 b = fin.position;
        Vector3 eje = new Vector3(b.x - a.x, 0f, b.z - a.z);
        float largo = eje.magnitude;
        Vector3 adelante = eje.normalized;
        Vector3 costado = Vector3.Cross(Vector3.up, adelante);
        Terrain terreno = Terrain.activeTerrain;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null) continue;

            float alo = Random.Range(-margenExtremos, largo + margenExtremos);
            float lado = Random.value < 0.5f ? -1f : 1f;
            float distancia = Random.Range(distanciaMinima, distanciaMaxima);
            Vector3 pos = a + adelante * alo + costado * (lado * distancia);
            if (terreno != null) pos.y = terreno.SampleHeight(pos) + terreno.transform.position.y;

            GameObject arbol = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
            arbol.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            arbol.transform.localScale = Vector3.one * Random.Range(escalaMinima, escalaMaxima);
            GameObjectUtility.SetStaticEditorFlags(arbol, (StaticEditorFlags)~0);
            Undo.RegisterCreatedObjectUndo(arbol, "Plantar arbol");
        }

        Random.state = estadoAnterior;
        Undo.CollapseUndoOperations(undoGroup);
    }

    // Busca (o crea) el padre en la escena activa y borra lo que tenia adentro.
    private Transform PrepararPadre()
    {
        GameObject padre = GameObject.Find(nombrePadre);
        if (padre == null)
        {
            padre = new GameObject(nombrePadre);
            Undo.RegisterCreatedObjectUndo(padre, "Crear padre de arboles");
        }

        for (int i = padre.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(padre.transform.GetChild(i).gameObject);
        }
        return padre.transform;
    }
}
