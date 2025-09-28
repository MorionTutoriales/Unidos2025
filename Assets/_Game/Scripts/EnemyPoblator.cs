using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPoblator : MonoBehaviour
{
    [Header("Referencias")]
    public MeshGenerator meshGenerator;
    public Transform jugador;
    public Transform boss;
    public GameObject[] enemigosPrefabs;
    public Transform contenedorEnemigos;

    [Header("Población")]
    public int cantidadEnemigos = 20;
    public float separacionMinEntreEnemigos = 2.0f;
    public float distanciaMinBossJugador = 25f;
    public float distanciaMinJugadorEnemigo = 8f;
    public float distanciaMinBossEnemigo = 6f;

    [Header("Ajustes de colocación")]
    public float yOffset = 0.1f;
    public bool validarNavMesh = true;      // Se auto-apaga si no hay navmesh listo
    public float navMeshMaxDist = 2.0f;

    [Header("Aleatoriedad")]
    public int seed = 0;

    [Header("Gizmos")]
    public bool mostrarGizmos = true;
    public Color gizmoJugador = new Color(0.2f, 0.8f, 1f, 0.9f);
    public Color gizmoBoss = new Color(1f, 0.3f, 0.3f, 0.9f);
    public float gizmoRadio = 0.6f;

    // Cache
    private List<Vector3> puntosSuelo = new List<Vector3>();
    private Vector3 posInicialJugador;
    private Vector3 posInicialBoss;
    private readonly List<Vector3> posicionesEnemigos = new List<Vector3>();


    [ContextMenu("Crear")]
    public void Iniciar()
    {
        if (meshGenerator == null) meshGenerator = GetComponent<MeshGenerator>();
        if (meshGenerator == null || meshGenerator.squareGrid == null)
        {
            Debug.LogWarning("[EnemyPoblator] No hay MeshGenerator/squareGrid. Genera el mapa primero.");
            return;
        }
        if (jugador == null || boss == null)
        {
            Debug.LogWarning("[EnemyPoblator] Falta jugador/boss asignado.");
            return;
        }
        if (enemigosPrefabs == null || enemigosPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemyPoblator] Asigna al menos 1 prefab de enemigo.");
            return;
        }

        if (seed != 0) Random.InitState(seed);

        // Si no hay navmesh todavía, auto-desactivar el filtro para no quedarnos sin posiciones
        if (validarNavMesh && !HayNavMeshListo())
        {
            validarNavMesh = false;
            Debug.LogWarning("[EnemyPoblator] NavMesh no detectado todavía. Se desactiva la validación por esta ejecución.");
        }

        ConstruirPuntosDeSuelo();
        if (puntosSuelo.Count == 0)
        {
            Debug.LogWarning("[EnemyPoblator] No hay puntos de suelo detectados.");
            return;
        }

        // Jugador
        posInicialJugador = ElegirPosAlAzarValida();
        jugador.position = AjustarY(posInicialJugador);

        // Boss lejos del jugador
        posInicialBoss = ElegirPosLejosDe(posInicialJugador, distanciaMinBossJugador, 400);
        // En mapas chicos, si no logra la distancia pedida, al menos garantiza que no quede pegado
        if ((posInicialBoss - posInicialJugador).sqrMagnitude < 9f)
            posInicialBoss = ForzarAlejadaDe(posInicialJugador, 3f);
        boss.position = AjustarY(posInicialBoss);

        // Poblar enemigos
        posicionesEnemigos.Clear();
        int creados = 0;
        int intentos = 0;
        int intentosMax = cantidadEnemigos * 30;

        while (creados < cantidadEnemigos && intentos < intentosMax)
        {
            intentos++;
            Vector3 p = ElegirPosEnemigoValida();
            if (!EsFinito(p)) continue;

            // Instanciar
            var prefab = enemigosPrefabs[Random.Range(0, enemigosPrefabs.Length)];
            var parent = contenedorEnemigos != null ? contenedorEnemigos : transform;
            Instantiate(prefab, AjustarY(p), Quaternion.identity, parent);
            posicionesEnemigos.Add(p);
            creados++;
        }

        if (creados == 0)
        {
            // Relajar restricciones en caso extremo
            Debug.LogWarning("[EnemyPoblator] No se pudieron crear enemigos con las restricciones actuales. Relajando distancias.");
            float oldJ = distanciaMinJugadorEnemigo, oldB = distanciaMinBossEnemigo, oldE = separacionMinEntreEnemigos;
            distanciaMinJugadorEnemigo = Mathf.Max(2f, oldJ * 0.5f);
            distanciaMinBossEnemigo = Mathf.Max(2f, oldB * 0.5f);
            separacionMinEntreEnemigos = Mathf.Max(1.5f, oldE * 0.7f);

            // Reintento rápido
            intentos = 0;
            while (creados < Mathf.Max(5, cantidadEnemigos / 2) && intentos < intentosMax)
            {
                intentos++;
                Vector3 p = ElegirPosEnemigoValida();
                if (!EsFinito(p)) continue;

                var prefab = enemigosPrefabs[Random.Range(0, enemigosPrefabs.Length)];
                var parent = contenedorEnemigos != null ? contenedorEnemigos : transform;
                Instantiate(prefab, AjustarY(p), Quaternion.identity, parent);
                posicionesEnemigos.Add(p);
                creados++;
            }
        }
    }

    // --------- Construcción de puntos de suelo ----------
    private void ConstruirPuntosDeSuelo()
    {
        puntosSuelo.Clear();
        var grid = meshGenerator.squareGrid;
        int maxX = grid.squares.GetLength(0);
        int maxY = grid.squares.GetLength(1);

        // Set para evitar duplicados por posiciones muy cercanas
        var uniq = new HashSet<Vector2Int>();

        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                var s = grid.squares[x, y];

                // corners que sean suelo (active == false)
                AgregarSiSuelo(uniq, s.topLeft);
                AgregarSiSuelo(uniq, s.topRight);
                AgregarSiSuelo(uniq, s.bottomRight);
                AgregarSiSuelo(uniq, s.bottomLeft);

                // centro del square si los 4 corners son suelo → buen punto medio
                if (!s.topLeft.active && !s.topRight.active && !s.bottomRight.active && !s.bottomLeft.active)
                {
                    Vector3 center = (s.topLeft.position + s.bottomRight.position) * 0.5f;
                    Agregar(uniq, center);
                }
            }
        }
    }

    private void AgregarSiSuelo(HashSet<Vector2Int> uniq, MeshGenerator.ControlNode n)
    {
        if (n != null && n.active == false) Agregar(uniq, n.position);
    }

    private void Agregar(HashSet<Vector2Int> uniq, Vector3 p)
    {
        // quantize para evitar duplicados (10 cm)
        var key = new Vector2Int(Mathf.RoundToInt(p.x * 10f), Mathf.RoundToInt(p.z * 10f));
        if (uniq.Add(key)) puntosSuelo.Add(p);
    }

    // --------- Selección de posiciones ----------
    private Vector3 ElegirPosAlAzarValida()
    {
        for (int i = 0; i < 200; i++)
        {
            var p = puntosSuelo[Random.Range(0, puntosSuelo.Count)];
            if (!EsFinito(p)) continue;
            if (!validarNavMesh || EnNavMeshCercano(p, out p)) return p;
        }
        // último recurso: cualquiera finita
        return puntosSuelo[Random.Range(0, puntosSuelo.Count)];
    }

    private Vector3 ElegirPosLejosDe(Vector3 referencia, float distanciaMin, int intentosMax)
    {
        Vector3 mejor = referencia;
        float mejorDist = -1f;

        for (int i = 0; i < intentosMax; i++)
        {
            var p = puntosSuelo[Random.Range(0, puntosSuelo.Count)];
            if (!EsFinito(p)) continue;
            if (validarNavMesh && !EnNavMeshCercano(p, out p)) continue;

            float d = Vector3.Distance(p, referencia);
            if (d >= distanciaMin) return p;
            if (d > mejorDist) { mejorDist = d; mejor = p; }
        }
        return mejor;
    }

    private Vector3 ForzarAlejadaDe(Vector3 referencia, float minDist)
    {
        for (int i = 0; i < 400; i++)
        {
            var p = puntosSuelo[Random.Range(0, puntosSuelo.Count)];
            if (!EsFinito(p)) continue;
            if ((p - referencia).sqrMagnitude >= minDist * minDist)
            {
                if (!validarNavMesh || EnNavMeshCercano(p, out p)) return p;
            }
        }
        return referencia + new Vector3(minDist, 0, minDist);
    }

    private Vector3 ElegirPosEnemigoValida()
    {
        for (int i = 0; i < 200; i++)
        {
            var p = puntosSuelo[Random.Range(0, puntosSuelo.Count)];
            if (!EsFinito(p)) continue;
            if (validarNavMesh && !EnNavMeshCercano(p, out p)) continue;

            if (Vector3.Distance(p, posInicialJugador) < distanciaMinJugadorEnemigo) continue;
            if (Vector3.Distance(p, posInicialBoss) < distanciaMinBossEnemigo) continue;

            bool ok = true;
            for (int k = 0; k < posicionesEnemigos.Count; k++)
                if (Vector3.Distance(p, posicionesEnemigos[k]) < separacionMinEntreEnemigos) { ok = false; break; }

            if (ok) return p;
        }
        return Vector3.negativeInfinity;
    }

    // --------- Utilidades ----------
    private static bool EsFinito(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
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

    private static bool HayNavMeshListo()
    {
        var tri = NavMesh.CalculateTriangulation();
        return tri.vertices != null && tri.vertices.Length > 0 && tri.indices != null && tri.indices.Length > 0;
    }

    private Vector3 AjustarY(Vector3 p) => new Vector3(p.x, p.y + yOffset, p.z);

    // --------- Gizmos ----------
    private void OnDrawGizmos()
    {
        if (!mostrarGizmos) return;

        if (posInicialJugador != Vector3.zero)
        {
            Gizmos.color = gizmoJugador;
            Gizmos.DrawSphere(AjustarY(posInicialJugador), gizmoRadio);
        }
        if (posInicialBoss != Vector3.zero)
        {
            Gizmos.color = gizmoBoss;
            Gizmos.DrawSphere(AjustarY(posInicialBoss), gizmoRadio);
        }
    }
}
