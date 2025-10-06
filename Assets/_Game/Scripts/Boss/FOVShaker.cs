using UnityEngine;
using Unity.Cinemachine;

public class FOVShaker : MonoBehaviour
{
    public static FOVShaker Instance { get; private set; }

    [Header("Referencia a la cámara Cinemachine")]
    public CinemachineCamera cineCam;

    [Header("Configuración")]
    public float minFOV = 10f;
    public float maxFOV = 120f;

    private float baseFOV;
    private Coroutine shakeRoutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (cineCam == null)
            cineCam = FindAnyObjectByType<CinemachineCamera>();

        if (cineCam != null)
            baseFOV = cineCam.Lens.FieldOfView;
    }

    /// <summary>
    /// Pulso rápido (ensanche y retorno del FOV)
    /// </summary>
    public void Pulse(float intensity = 10f, float duration = 0.3f)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(PulseRoutine(intensity, duration));
    }

    /// <summary>
    /// Vibración tipo shake en el FOV
    /// </summary>
    public void Shake(float amplitude = 5f, float frequency = 20f, float duration = 0.5f)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(amplitude, frequency, duration));
    }

    private System.Collections.IEnumerator PulseRoutine(float intensity, float duration)
    {
        if (cineCam == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            float current = Mathf.Sin(progress * Mathf.PI); // subida y bajada
            float newFOV = Mathf.Clamp(baseFOV + current * intensity, minFOV, maxFOV);
            SetFOV(newFOV);
            yield return null;
        }

        SetFOV(baseFOV);
        shakeRoutine = null;
    }

    private System.Collections.IEnumerator ShakeRoutine(float amplitude, float frequency, float duration)
    {
        if (cineCam == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float noise = Mathf.PerlinNoise(t * frequency, 0f) * 2f - 1f; // -1 a 1
            float newFOV = Mathf.Clamp(baseFOV + noise * amplitude, minFOV, maxFOV);
            SetFOV(newFOV);
            yield return null;
        }

        SetFOV(baseFOV);
        shakeRoutine = null;
    }

    private void SetFOV(float value)
    {
        if (cineCam != null)
        {
            var lens = cineCam.Lens;
            lens.FieldOfView = value; // cambia Vertical FOV
            cineCam.Lens = lens;
        }
    }
}
