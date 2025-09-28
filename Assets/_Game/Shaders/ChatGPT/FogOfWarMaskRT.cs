using UnityEngine;

/// Mantiene una máscara acumulativa en RT con ping-pong (no se borra lo revelado).
[RequireComponent(typeof(Renderer))]
public class FogOfWarMaskRT : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;              // Personaje
    public Material fogMaterialTemplate;  // Material con shader "URP/FogOfWarMultiply"
    [Tooltip("Material con shader Hidden/FOW_PaintMaxCircle (pinta y hace max con lo previo)")]
    public Material paintMaterial;

    [Header("Plano que cubre el mapa (XZ)")]
    public Vector2 worldSize = new Vector2(100, 100);
    public Vector3 worldOrigin = Vector3.zero;

    [Header("Máscara")]
    [Min(64)] public int maskResolution = 1024;
    public float revealRadiusWorld = 5f;
    public float featherWorld = 2f;

    [Header("Oscurecimiento")]
    [Range(0, 1)] public float minFactor = 0.0f; // 0 = negro total

    static readonly int _MaskID = Shader.PropertyToID("_Mask");
    static readonly int _MinID = Shader.PropertyToID("_MinFactor");
    static readonly int _CenterID = Shader.PropertyToID("_CenterUV");
    static readonly int _RadiusID = Shader.PropertyToID("_Radius");
    static readonly int _FeatherID = Shader.PropertyToID("_Feather");

    Renderer rend;
    Material fogMatInstance;   // instancia propia (no shared)
    RenderTexture maskA, maskB; // ping-pong

    void Reset()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            var b = rend.bounds;
            worldOrigin = new Vector3(b.min.x, 0, b.min.z);
            worldSize = new Vector2(b.size.x, b.size.z);
        }
    }

    void Awake()
    {
        rend = GetComponent<Renderer>();

        // 1) Crear instancia de material (para que nadie más lo pise)
        if (fogMaterialTemplate != null) fogMatInstance = new Material(fogMaterialTemplate);
        else if (rend != null) fogMatInstance = new Material(rend.sharedMaterial);
        if (rend != null) rend.material = fogMatInstance;

        // 2) Crear doble buffer (ARGB32 para compatibilidad; sin mips)
        maskA = CreateMaskRT(maskResolution);
        maskB = CreateMaskRT(maskResolution);
        ClearRT(maskA, Color.black);
        ClearRT(maskB, Color.black);

        // 3) Conectar la RT al material
        fogMatInstance.SetTexture(_MaskID, maskA);
        fogMatInstance.SetFloat(_MinID, Mathf.Clamp01(minFactor));
    }

    RenderTexture CreateMaskRT(int res)
    {
        var rt = new RenderTexture(res, res, 0, RenderTextureFormat.ARGB32)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            useMipMap = false,
            autoGenerateMips = false
        };
        rt.Create();
        return rt;
    }

    void ClearRT(RenderTexture rt, Color c)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(false, true, c);
        RenderTexture.active = prev;
    }

    void OnDestroy()
    {
        if (maskA) { maskA.Release(); Destroy(maskA); }
        if (maskB) { maskB.Release(); Destroy(maskB); }
        if (fogMatInstance) Destroy(fogMatInstance);
    }

    void LateUpdate()
    {
        if (!player || paintMaterial == null || maskA == null || maskB == null || fogMatInstance == null) return;

        fogMatInstance.SetFloat(_MinID, Mathf.Clamp01(minFactor));

        // Mundo → UV (0..1)
        Vector3 p = player.position;
        float u = Mathf.InverseLerp(worldOrigin.x, worldOrigin.x + worldSize.x, p.x);
        float v = Mathf.InverseLerp(worldOrigin.z, worldOrigin.z + worldSize.y, p.z);
        Vector2 uv = new Vector2(u, v);

        // Radios en UV
        float norm = Mathf.Max(worldSize.x, worldSize.y);
        float rUV = revealRadiusWorld / norm;
        float fUV = Mathf.Max(0.0001f, featherWorld / norm);

        paintMaterial.SetVector(_CenterID, uv);
        paintMaterial.SetFloat(_RadiusID, rUV);
        paintMaterial.SetFloat(_FeatherID, fUV);

        // --- Ping-Pong persistente ---
        // Lee de maskA (_MainTex) y escribe en maskB el "max" con el círculo
        Graphics.Blit(maskA, maskB, paintMaterial);

        // Conecta maskB como la nueva máscara visible
        fogMatInstance.SetTexture(_MaskID, maskB);

        // Intercambia referencias (maskA <= contenido actualizado; maskB queda como la visible)
        var tmp = maskA; maskA = maskB; maskB = tmp;
    }
}
