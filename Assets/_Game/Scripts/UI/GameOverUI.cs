using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup backgroundPanel;
    [SerializeField] private Image logoImage;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("Animation Settings")]
    [SerializeField] private float backgroundFadeDuration = 0.6f;
    [SerializeField] private float logoAnimationDuration = 0.8f;
    [SerializeField] private float buttonAnimationDuration = 0.5f;
    [SerializeField] private float delayBetweenButtons = 0.15f;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve backgroundCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve logoCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve buttonCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scene Names")]
    [SerializeField] private string currentSceneName = "GameScene";
    [SerializeField] private string menuSceneName = "MainMenu";

    private RectTransform logoRect;
    private RectTransform restartRect;
    private RectTransform menuRect;
    private CanvasGroup logoCanvasGroup;
    private CanvasGroup restartCanvasGroup;
    private CanvasGroup menuCanvasGroup;

    private Vector2 logoStartPos;
    private Vector2 restartStartPos;
    private Vector2 menuStartPos;

    private bool isAnimating = false;

    public static GameOverUI singleton;

    private void Awake()
    {
        SetupComponents();
        SetupButtonListeners();
        HideAllElements();
        singleton = this;
    }

    private void SetupComponents()
    {
        // Logo
        logoRect = logoImage.GetComponent<RectTransform>();
        logoCanvasGroup = logoImage.gameObject.GetComponent<CanvasGroup>();
        if (logoCanvasGroup == null)
            logoCanvasGroup = logoImage.gameObject.AddComponent<CanvasGroup>();

        // Restart Button
        restartRect = restartButton.GetComponent<RectTransform>();
        restartCanvasGroup = restartButton.gameObject.GetComponent<CanvasGroup>();
        if (restartCanvasGroup == null)
            restartCanvasGroup = restartButton.gameObject.AddComponent<CanvasGroup>();

        // Menu Button
        menuRect = menuButton.GetComponent<RectTransform>();
        menuCanvasGroup = menuButton.gameObject.GetComponent<CanvasGroup>();
        if (menuCanvasGroup == null)
            menuCanvasGroup = menuButton.gameObject.AddComponent<CanvasGroup>();

        // Store initial positions
        logoStartPos = logoRect.anchoredPosition;
        restartStartPos = restartRect.anchoredPosition;
        menuStartPos = menuRect.anchoredPosition;
    }

    private void SetupButtonListeners()
    {
        restartButton.onClick.AddListener(RestartGame);
        menuButton.onClick.AddListener(GoToMenu);
    }

    private void HideAllElements()
    {
        StartCoroutine(FadeInicial());
        backgroundPanel.blocksRaycasts = false;

        logoCanvasGroup.alpha = 0;
        logoRect.anchoredPosition = logoStartPos + Vector2.up * 100f;
        logoRect.localScale = Vector3.one * 0.5f;

        restartCanvasGroup.alpha = 0;
        restartRect.anchoredPosition = restartStartPos + Vector2.left * 200f;
        restartRect.localScale = Vector3.one * 0.8f;
        restartButton.interactable = false;

        menuCanvasGroup.alpha = 0;
        menuRect.anchoredPosition = menuStartPos + Vector2.right * 200f;
        menuRect.localScale = Vector3.one * 0.8f;
        menuButton.interactable = false;
    }

    public IEnumerator FadeInicial()
    {

        float elapsed = 0f;

        while (elapsed < 2*backgroundFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (2*backgroundFadeDuration);
            float curveValue = 1- backgroundCurve.Evaluate(t);

            backgroundPanel.alpha = curveValue; // Fondo casi opaco

            yield return null;
        }

        backgroundPanel.alpha = 0f;
    }

    // Método público para activar el Game Over
    [ContextMenu("Test Game Over")]
    public void TriggerGameOver()
    {
        if (isAnimating) return;
        StartCoroutine(GameOverSequence());
    }

    // Método público para hacer Fade Out (mantiene el fondo negro)
    [ContextMenu("Test Fade Out")]
    public void FadeOutUI()
    {
        if (isAnimating) return;
        StartCoroutine(FadeOutSequence());
    }

    private IEnumerator GameOverSequence()
    {
        isAnimating = true;
        Time.timeScale = 1f; // Asegurar que el tiempo está correcto

        // Bloquear interacciones durante la animación
        backgroundPanel.blocksRaycasts = true;

        // Paso 1: Fade in del fondo negro
        yield return StartCoroutine(AnimateBackground());

        // Paso 2: Animar el logo con efecto de entrada dramático
        yield return StartCoroutine(AnimateLogo());

        // Paso 3: Animar botones de forma escalonada
        StartCoroutine(AnimateButton(restartRect, restartCanvasGroup, restartButton, restartStartPos, 0f));
        yield return StartCoroutine(AnimateButton(menuRect, menuCanvasGroup, menuButton, menuStartPos, delayBetweenButtons));

        isAnimating = false;
    }

    private IEnumerator AnimateBackground()
    {
        float elapsed = 0f;

        while (elapsed < backgroundFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / backgroundFadeDuration;
            float curveValue = backgroundCurve.Evaluate(t);

            backgroundPanel.alpha = curveValue * 0.95f; // Fondo casi opaco

            yield return null;
        }

        backgroundPanel.alpha = 0.95f;
    }


    private IEnumerator AnimateBackgroundFull()
    {
        float elapsed = backgroundPanel.alpha;

        while (elapsed < backgroundFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / backgroundFadeDuration;
            float curveValue = backgroundCurve.Evaluate(t);

            backgroundPanel.alpha = curveValue ; // Fondo casi opaco

            yield return null;
        }

        backgroundPanel.alpha = 1f;
    }

    private IEnumerator AnimateLogo()
    {
        float elapsed = 0f;
        Vector2 startPos = logoRect.anchoredPosition;
        Vector3 startScale = logoRect.localScale;

        while (elapsed < logoAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / logoAnimationDuration;
            float curveValue = logoCurve.Evaluate(t);

            // Movimiento suave desde arriba (solo eje Y)
            logoRect.anchoredPosition = Vector2.Lerp(startPos, logoStartPos, curveValue);

            // Escala con efecto de "pop"
            float scaleMultiplier = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f; // Bounce effect
            logoRect.localScale = Vector3.Lerp(startScale, Vector3.one * scaleMultiplier, curveValue);

            // Fade in
            logoCanvasGroup.alpha = curveValue;

            yield return null;
        }

        // Finalizar en valores exactos
        logoRect.anchoredPosition = logoStartPos;
        logoRect.localScale = Vector3.one;
        logoCanvasGroup.alpha = 1f;
    }

    private IEnumerator AnimateButton(RectTransform rect, CanvasGroup canvasGroup, Button button, Vector2 targetPos, float initialDelay)
    {
        if (initialDelay > 0)
            yield return new WaitForSeconds(initialDelay);

        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Vector3 startScale = rect.localScale;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = buttonCurve.Evaluate(t);

            // Movimiento desde los lados con efecto elástico
            float elasticEffect = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue * elasticEffect);

            // Escala suave
            rect.localScale = Vector3.Lerp(startScale, Vector3.one, curveValue);

            // Fade in
            canvasGroup.alpha = curveValue;

            yield return null;
        }

        // Finalizar
        rect.anchoredPosition = targetPos;
        rect.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        button.interactable = true;

        // Efecto de "hover" sutil al finalizar
        StartCoroutine(ButtonIdleAnimation(rect));
    }

    private IEnumerator ButtonIdleAnimation(RectTransform rect)
    {
        float time = 0f;
        Vector3 originalScale = rect.localScale;

        while (true)
        {
            time += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(time * 2f) * 0.02f; // Pulsación muy sutil
            rect.localScale = originalScale * pulse;
            yield return null;
        }
    }

    // Funciones de los botones
    private void RestartGame()
    {
        Time.timeScale = 1f;
        FadeOutUI();
        Invoke("ReinicarEscena", 2);
    }

    void ReinicarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // Método opcional para resetear el UI
    public void ResetUI()
    {
        StopAllCoroutines();
        isAnimating = false;
        HideAllElements();
    }

    // Secuencia de Fade Out (solo logo y botones, mantiene el fondo)
    private IEnumerator FadeOutSequence()
    {
        isAnimating = true;

        // Deshabilitar botones inmediatamente
        restartButton.interactable = false;
        menuButton.interactable = false;

        // Animar logo saliendo
        yield return StartCoroutine(FadeOutLogo());

        // Esperar que los botones salgan
        // Animar botones saliendo (inverso de la entrada)
        StartCoroutine(FadeOutButton(restartRect, restartCanvasGroup, Vector2.left * 200f, 0f));
        StartCoroutine(FadeOutButton(menuRect, menuCanvasGroup, Vector2.right * 200f, delayBetweenButtons));
        yield return new WaitForSeconds(buttonAnimationDuration);
        StartCoroutine(AnimateBackgroundFull());


        isAnimating = false;
    }

    private IEnumerator FadeOutLogo()
    {
        float elapsed = 0f;
        Vector2 startPos = logoRect.anchoredPosition;
        Vector2 targetPos = logoStartPos + Vector2.up * 100f;
        Vector3 startScale = logoRect.localScale;

        while (elapsed < logoAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / logoAnimationDuration;
            float curveValue = logoCurve.Evaluate(t);

            // Movimiento suave hacia arriba
            logoRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);

            // Reducir escala
            logoRect.localScale = Vector3.Lerp(startScale, Vector3.one * 0.5f, curveValue);

            // Fade out
            logoCanvasGroup.alpha = 1f - curveValue;

            yield return null;
        }

        // Finalizar
        logoRect.anchoredPosition = targetPos;
        logoRect.localScale = Vector3.one * 0.5f;
        logoCanvasGroup.alpha = 0f;
    }

    private IEnumerator FadeOutButton(RectTransform rect, CanvasGroup canvasGroup, Vector2 direction, float initialDelay)
    {
        if (initialDelay > 0)
            yield return new WaitForSeconds(initialDelay);

        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 targetPos = startPos + direction;
        Vector3 startScale = rect.localScale;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = buttonCurve.Evaluate(t);

            // Movimiento hacia los lados
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);

            // Reducir escala
            rect.localScale = Vector3.Lerp(startScale, Vector3.one * 0.8f, curveValue);

            // Fade out
            canvasGroup.alpha = 1f - curveValue;

            yield return null;
        }

        // Finalizar
        rect.anchoredPosition = targetPos;
        rect.localScale = Vector3.one * 0.8f;
        canvasGroup.alpha = 0f;
    }
}