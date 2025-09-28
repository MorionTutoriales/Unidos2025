using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FogOfWarSimple : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;       // Tu personaje
    public Material fogMaterial;   // Material con shader "URP/FogOfWarSimple"

    [Header("Parámetros (opcionales)")]
    public float radius = 4f;
    public float feather = 1.5f;
    [Range(0f, 1.5f)] public float strength = 1f;

    static readonly int PlayerPosID = Shader.PropertyToID("_PlayerWorldPos");
    static readonly int RadiusID = Shader.PropertyToID("_Radius");
    static readonly int FeatherID = Shader.PropertyToID("_Feather");
    static readonly int StrengthID = Shader.PropertyToID("_Strength");

    void Reset()
    {
        var r = GetComponent<Renderer>();
        if (r != null) fogMaterial = r.sharedMaterial;
    }

    void LateUpdate()
    {
        if (!player || !fogMaterial) return;

        fogMaterial.SetVector(PlayerPosID, player.position);
        fogMaterial.SetFloat(RadiusID, radius);
        fogMaterial.SetFloat(FeatherID, Mathf.Max(0.001f, feather));
        fogMaterial.SetFloat(StrengthID, (strength));
    }
}
