using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class NieblaGuerra : MonoBehaviour
{
    [Header("Referencias")]
    public Transform personaje;          // Personaje a seguir
    public Material materialNiebla;      // Material con shader "URP/NieblaG"

    [Header("Plano que cubre el mapa (XZ)")]
    public Vector2 tamanoMundo = new Vector2(100, 100); // tamaño del nivel en XZ
    public Vector3 origenMundo = Vector3.zero;          // esquina inferior-izquierda (min XZ)

    [Header("Máscara")]
    [Min(64)] public int resolucion = 1024;     // resolución de la imagen/máscara
    public float radioRevelado = 5f;            // radio del círculo en unidades de mundo
    public float suavidadBorde = 2f;            // ancho del borde suave en unidades de mundo (0 = borde duro)
    public float distanciaRepintado = 0.5f;     // distancia mínima que debe moverse el personaje para volver a pintar

    [Header("Oscurecimiento")]
    [Range(0, 1)] public float oscuridadMinima = 0.0f; // 0 = negro total, 1 = sin oscurecer

    // IDs del shader
    static readonly int MaskID = Shader.PropertyToID("_Mask");
    static readonly int MinFactorID = Shader.PropertyToID("_MinFactor");

    Renderer rend;
    Texture2D mascara;           // imagen de máscara (R: 0..1)
    Color32[] buffer;            // buffer CPU
    bool sucia;                  // hay cambios en buffer que subir a GPU

    Vector3 ultimaPosPintada;    // última posición del personaje cuando se pintó
    bool primeraPintura = true;

    void Reset()
    {
        rend = GetComponent<Renderer>();
        if (!materialNiebla && rend) materialNiebla = rend.sharedMaterial;

        // Si el plano ya está colocado/escala hecha, autocalcula origen y tamaño desde bounds:
        if (rend != null)
        {
            var b = rend.bounds;
            origenMundo = new Vector3(b.min.x, 0, b.min.z);
            tamanoMundo = new Vector2(b.size.x, b.size.z);
        }
    }

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (!materialNiebla && rend) materialNiebla = rend.sharedMaterial;

        // Crear textura de máscara (inicialmente negra = todo oculto)
        mascara = new Texture2D(resolucion, resolucion, TextureFormat.R8, false, true);
        mascara.wrapMode = TextureWrapMode.Clamp;
        mascara.filterMode = FilterMode.Bilinear;

        buffer = new Color32[resolucion * resolucion];
        for (int i = 0; i < buffer.Length; i++) buffer[i] = new Color32(0, 0, 0, 255);
        mascara.SetPixels32(buffer);
        mascara.Apply(false, false);

        if (materialNiebla)
        {
            materialNiebla.SetTexture(MaskID, mascara);
            materialNiebla.SetFloat(MinFactorID, Mathf.Clamp01(oscuridadMinima));
        }
    }

    void OnDestroy()
    {
        if (mascara != null) Destroy(mascara);
        buffer = null;
    }

    void LateUpdate()
    {
        if (!personaje || !materialNiebla || mascara == null || buffer == null) return;

        // Actualiza oscuridad por si la cambias en runtime
        materialNiebla.SetFloat(MinFactorID, Mathf.Clamp01(oscuridadMinima));

        // ¿Nos movimos lo suficiente desde la última pintura?
        if (primeraPintura || (personaje.position - ultimaPosPintada).sqrMagnitude >= distanciaRepintado * distanciaRepintado)
        {
            PintarCirculoSuave(personaje.position);
            ultimaPosPintada = personaje.position;
            primeraPintura = false;
        }

        if (sucia)
        {
            mascara.SetPixels32(buffer);
            mascara.Apply(false, false);
            sucia = false;
        }
    }

    void PintarCirculoSuave(Vector3 posMundo)
    {
        // Mundo -> UV [0..1] en plano
        float u = Mathf.InverseLerp(origenMundo.x, origenMundo.x + tamanoMundo.x, posMundo.x);
        float v = Mathf.InverseLerp(origenMundo.z, origenMundo.z + tamanoMundo.y, posMundo.z);

        // UV -> pixeles
        int cx = Mathf.Clamp(Mathf.RoundToInt(u * (resolucion - 1)), 0, resolucion - 1);
        int cy = Mathf.Clamp(Mathf.RoundToInt(v * (resolucion - 1)), 0, resolucion - 1);

        // Radio y suavidad en pixeles (normalizamos por el mayor lado)
        float norm = Mathf.Max(tamanoMundo.x, tamanoMundo.y);
        float rUV = radioRevelado / Mathf.Max(0.0001f, norm);
        float fUV = Mathf.Max(0f, suavidadBorde) / Mathf.Max(0.0001f, norm);

        int rPx = Mathf.CeilToInt(rUV * (resolucion - 1));
        int fPx = Mathf.CeilToInt(fUV * (resolucion - 1));

        int rTotal = rPx + fPx;
        int x0 = Mathf.Max(0, cx - rTotal);
        int x1 = Mathf.Min(resolucion - 1, cx + rTotal);
        int y0 = Mathf.Max(0, cy - rTotal);
        int y1 = Mathf.Min(resolucion - 1, cy + rTotal);

        float r = rPx;
        float rIn = Mathf.Max(0.0f, rPx - fPx); // inicio del feather
        float r2 = r * r;
        float rIn2 = rIn * rIn;

        for (int y = y0; y <= y1; y++)
        {
            int dy = y - cy;
            int dy2 = dy * dy;
            int row = y * resolucion;

            for (int x = x0; x <= x1; x++)
            {
                int dx = x - cx;
                int dx2 = dx * dx;
                float d2 = dx2 + dy2;

                float t; // 0 fuera, 1 centro
                if (d2 <= rIn2) t = 1f;
                else if (d2 >= r2) t = 0f;
                else
                {
                    float d = Mathf.Sqrt(d2);
                    float a = Mathf.Clamp01((r - d) / Mathf.Max(1e-5f, r - rIn)); // 0..1
                    t = a;
                }

                if (t > 0f)
                {
                    int idx = row + x;
                    byte cur = buffer[idx].r;
                    byte val = (byte)Mathf.Max(cur, Mathf.RoundToInt(t * 255f)); // max → NO borra lo revelado
                    if (val != cur)
                    {
                        buffer[idx].r = val;
                        sucia = true;
                    }
                }
            }
        }
    }
}
