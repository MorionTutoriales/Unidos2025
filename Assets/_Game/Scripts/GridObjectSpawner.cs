using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GridObjectSpawner : MonoBehaviour
{
    [Header("Referencias (asigna al menos una)")]
    public MapGenerator mapGenerator;
    public MeshGenerator meshGenerator;

    [Header("Prefabs a instanciar")]
    public GameObject[] prefabs;

    [Header("Dónde colocar")]
    [Tooltip("Si está activo: coloca en nodos ACTivos (paredes). Si no: en nodos INACTivos (suelo).")]
    public bool colocarEnActivos = false;
    [Tooltip("Usar el centro del square cuando TODOS los corners cumplen la condición.")]
    public bool usarCentros = true;

    [Header("Reglas de instanciación")]
    [Min(0)] public int maxObjetos = 200;          // límite superior
    public float separacionMinima = 0f;            // 0 = sin restricción
    public float yOffset = 0.1f;
    public Transform contenedor;
    public bool limpiarPrevios = true;
    public int seed = 0;                            // 0 = aleatorio del sistema

    [Header("Validaciones opcionales")]
    public bool validarNavMesh = false;
    public float navMeshMaxDist = 2f;

    [Header("Gizmos")]
    public bool dibujarGizmos = true;
    public Color gizmoColor = new Color(0.2f, 1f, 0.4f, 0.7f);
    public float gizmoRadio = 0.2f;

    // Cache
    private readonly List<Vector3> candidatos = new List<Vector3>();
    private readonly List<Vector3> usados = new List<Vector3>();

    [ContextMenu("Iniciar")]
    public void Iniciar()
    {
        // Resolver referencia a MeshGenerator desde MapGenerator si es necesario
        if (meshGenerator == null && mapGenerator != null)
            meshGenerator = mapGenerator.GetComponent<MeshGenerator>();

        if (meshGenerator == null || meshGenerator.squareGrid == null)
        {
            Debug.LogWarning("[GridObjectSpawner] Necesito un MeshGenerator con squareGrid listo.");
            return;
        }
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("[GridObjectSpawner] Asigna al menos un prefab.");
            return;
        }

        if (seed != 0) Random.InitState(seed);

        // Limpiar instancias previas (si se pide)
        if (limpiarPrevios && contenedor != null)
        {
            var borrar = new List<GameObject>();
            for (int i = contenedor.childCount - 1; i >= 0; i--)
                borrar.Add(contenedor.GetChild(i).gameObject);

            for (int i = 0; i < borrar.Count; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(borrar[i]);
                else Destroy(borrar[i]);
#else
                Destroy(borrar[i]);
#endif
            }
        }

        // Construir y mezclar candidatos
        ConstruirCandidatos();
        MezclarFisherYates(candidatos);  // <- clave: desordenar el recorrido globalmente

        // Instanciar hasta maxObjetos recorriendo candidatos desordenados
        usados.Clear();
        int creados = 0;
        Transform parent = contenedor != null ? contenedor : transform;

        for (int i = 0; i < candidatos.Count && creados < maxObjetos; i++)
        {
            Vector3 p = candidatos[i];

            // Validación NavMesh (opcional)
            if (validarNavMesh && !EnNavMeshCercano(p, out p)) continue;

            // Separación mínima entre instancias ya colocadas (opcional)
            if (separacionMinima > 0f && MuyCercaDeUsados(p, separacionMinima)) continue;

            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            var pos = new Vector3(p.x, p.y + yOffset, p.z);

            Instantiate(prefab, pos, Quaternion.identity, parent);
            usados.Add(p);
            creados++;
        }

        Debug.Log($"[GridObjectSpawner] Candidatos: {candidatos.Count} | Creados: {creados} (máx {maxObjetos})");
    }

    // -----------------------------
    // Construcción de candidatos
    // -----------------------------
    private void ConstruirCandidatos()
    {
        candidatos.Clear();
        var grid = meshGenerator.squareGrid;
        int maxX = grid.squares.GetLength(0);
        int maxY = grid.squares.GetLength(1);

        var uniq = new HashSet<Vector2Int>(); // evita duplicados por float

        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                var s = grid.squares[x, y];

                AgregarSiCoincide(uniq, s.topLeft);
                AgregarSiCoincide(uniq, s.topRight);
                AgregarSiCoincide(uniq, s.bottomRight);
                AgregarSiCoincide(uniq, s.bottomLeft);

                if (usarCentros)
                {
                    bool cornersActivos = s.topLeft.active && s.topRight.active && s.bottomRight.active && s.bottomLeft.active;
                    bool cornersInactivos = !s.topLeft.active && !s.topRight.active && !s.bottomRight.active && !s.bottomLeft.active;

                    if ((colocarEnActivos && cornersActivos) || (!colocarEnActivos && cornersInactivos))
                    {
                        Vector3 centro = (s.topLeft.position + s.bottomRight.position) * 0.5f;
                        Agregar(uniq, centro);
                    }
                }
            }
        }
    }

    private void AgregarSiCoincide(HashSet<Vector2Int> uniq, MeshGenerator.ControlNode n)
    {
        if (n == null) return;
        if ((colocarEnActivos && n.active) || (!colocarEnActivos && !n.active))
            Agregar(uniq, n.position);
    }

    private void Agregar(HashSet<Vector2Int> uniq, Vector3 p)
    {
        if (!EsFinito(p)) return;
        var key = new Vector2Int(Mathf.RoundToInt(p.x * 10f), Mathf.RoundToInt(p.z * 10f));
        if (uniq.Add(key)) candidatos.Add(p);
    }

    // -----------------------------
    // Utilidades
    // -----------------------------
    private static void MezclarFisherYates(List<Vector3> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private bool MuyCercaDeUsados(Vector3 p, float minDist)
    {
        float minSqr = minDist * minDist;
        for (int i = 0; i < usados.Count; i++)
            if ((usados[i] - p).sqrMagnitude < minSqr) return true;
        return false;
    }

    private bool EnNavMeshCercano(Vector3 origen, out Vector3 posValida)
    {
        posValida = origen;
        if (NavMesh.SamplePosition(origen, out var hit, navMeshMaxDist, NavMesh.AllAreas))
        {
            posValida = hit.position;
            return true;
        }
        return false;
    }

    private static bool EsFinito(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }

    private void OnDrawGizmosSelected()
    {
        if (!dibujarGizmos || candidatos == null) return;
        Gizmos.color = gizmoColor;
        for (int i = 0; i < candidatos.Count; i++)
            Gizmos.DrawSphere(new Vector3(candidatos[i].x, candidatos[i].y + yOffset, candidatos[i].z), gizmoRadio);
    }
}
