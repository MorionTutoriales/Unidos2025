using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup backgroundPanel;
    [SerializeField] private Button[] menuButtons; // Array de botones que aparecerán

    [Header("Animation Settings")]
    [SerializeField] private float backgroundFadeDuration = 0.4f;
    [SerializeField] private float buttonAnimationDuration = 0.4f;
    [SerializeField] private float delayBetweenButtons = 0.08f;
    [SerializeField] private float buttonSlideDistance = 150f;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve backgroundCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve buttonCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Background Settings")]
    [SerializeField] private float backgroundMaxAlpha = 0.85f;
    [SerializeField] private bool blurEffect = true; // Visual hint para blur

    private List<RectTransform> buttonRects = new List<RectTransform>();
    private List<CanvasGroup> buttonCanvasGroups = new List<CanvasGroup>();
    private List<Vector2> buttonOriginalPositions = new List<Vector2>();
    private List<Coroutine> buttonIdleCoroutines = new List<Coroutine>();

    private bool isOpen = false;
    private bool isAnimating = false;
    private float originalTimeScale;
    public static PauseMenuUI singleton;
    private void Awake()
    {
        SetupComponents();
        HideAllElements();
        singleton = this;
    }

    private void SetupComponents()
    {
        // Configurar cada botón
        foreach (Button button in menuButtons)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            buttonRects.Add(rect);
            buttonOriginalPositions.Add(rect.anchoredPosition);

            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            buttonCanvasGroups.Add(canvasGroup);

            // Efecto hover en los botones
            AddHoverEffect(button, rect);
        }
    }

    private void AddHoverEffect(Button button, RectTransform rect)
    {
        // Añadir efecto de escala al hacer hover
        UnityEngine.EventSystems.EventTrigger trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Pointer Enter
        UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { StartCoroutine(ScaleButton(rect, 1.1f, 0.15f)); });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit
        UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { StartCoroutine(ScaleButton(rect, 1.0f, 0.15f)); });
        trigger.triggers.Add(entryExit);
    }

    private IEnumerator ScaleButton(RectTransform rect, float targetScale, float duration)
    {
        Vector3 startScale = rect.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Usar unscaledDeltaTime por el pause
            float t = elapsed / duration;
            rect.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        rect.localScale = endScale;
    }

    private void HideAllElements()
    {
        backgroundPanel.alpha = 0;
        backgroundPanel.blocksRaycasts = false;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            buttonCanvasGroups[i].alpha = 0;
            buttonRects[i].anchoredPosition = buttonOriginalPositions[i] + Vector2.left * buttonSlideDistance;
            buttonRects[i].localScale = Vector3.one * 0.9f;
            menuButtons[i].interactable = false;
        }
    }

    [ContextMenu("Test Open Menu")]
    public void OpenMenu()
    {
        if (isOpen || isAnimating) return;
        StartCoroutine(OpenMenuSequence());
    }

    [ContextMenu("Test Close Menu")]
    public void CloseMenu()
    {
        if (!isOpen || isAnimating) return;
        StartCoroutine(CloseMenuSequence());
    }

    private IEnumerator OpenMenuSequence()
    {
        isAnimating = true;
        isOpen = true;

        // Guardar el timeScale actual
        originalTimeScale = Time.timeScale;

        backgroundPanel.blocksRaycasts = true;

        // Paso 1: Fade in del fondo
        yield return StartCoroutine(AnimateBackground(true));

        // Paso 2: Pausar el juego después del fondo
        Time.timeScale = 0f;

        // Paso 3: Animar botones de forma escalonada
        for (int i = 0; i < menuButtons.Length; i++)
        {
            StartCoroutine(AnimateButtonIn(i));
            yield return new WaitForSecondsRealtime(delayBetweenButtons);
        }

        // Esperar a que el último botón termine
        yield return new WaitForSecondsRealtime(buttonAnimationDuration);

        // Activar animaciones idle en los botones
        StartIdleAnimations();

        isAnimating = false;
    }

    private IEnumerator CloseMenuSequence()
    {
        isAnimating = true;

        // Detener animaciones idle
        StopIdleAnimations();

        // Deshabilitar botones inmediatamente
        foreach (Button button in menuButtons)
        {
            button.interactable = false;
        }

        // Animar botones saliendo de forma escalonada (en reversa)
        for (int i = menuButtons.Length - 1; i >= 0; i--)
        {
            StartCoroutine(AnimateButtonOut(i));
            yield return new WaitForSecondsRealtime(delayBetweenButtons);
        }

        // Esperar a que el último botón termine
        yield return new WaitForSecondsRealtime(buttonAnimationDuration);

        // Restaurar el timeScale ANTES del fade out
        Time.timeScale = originalTimeScale;

        // Fade out del fondo
        yield return StartCoroutine(AnimateBackground(false));

        backgroundPanel.blocksRaycasts = false;
        isOpen = false;
        isAnimating = false;
    }

    private IEnumerator AnimateBackground(bool fadeIn)
    {
        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : backgroundMaxAlpha;
        float endAlpha = fadeIn ? backgroundMaxAlpha : 0f;

        while (elapsed < backgroundFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / backgroundFadeDuration;
            float curveValue = backgroundCurve.Evaluate(t);

            backgroundPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);

            yield return null;
        }

        backgroundPanel.alpha = endAlpha;
    }

    private IEnumerator AnimateButtonIn(int index)
    {
        float elapsed = 0f;
        Vector2 startPos = buttonRects[index].anchoredPosition;
        Vector2 targetPos = buttonOriginalPositions[index];
        Vector3 startScale = buttonRects[index].localScale;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = buttonCurve.Evaluate(t);

            // Movimiento desde la izquierda con efecto elástico sutil
            float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f;
            buttonRects[index].anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue * overshoot);

            // Escala suave
            buttonRects[index].localScale = Vector3.Lerp(startScale, Vector3.one, curveValue);

            // Fade in
            buttonCanvasGroups[index].alpha = curveValue;

            yield return null;
        }

        // Finalizar
        buttonRects[index].anchoredPosition = targetPos;
        buttonRects[index].localScale = Vector3.one;
        buttonCanvasGroups[index].alpha = 1f;
        menuButtons[index].interactable = true;
    }

    private IEnumerator AnimateButtonOut(int index)
    {
        float elapsed = 0f;
        Vector2 startPos = buttonRects[index].anchoredPosition;
        Vector2 targetPos = buttonOriginalPositions[index] + Vector2.right * buttonSlideDistance;
        Vector3 startScale = buttonRects[index].localScale;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = buttonCurve.Evaluate(t);

            // Movimiento hacia la derecha
            buttonRects[index].anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);

            // Reducir escala
            buttonRects[index].localScale = Vector3.Lerp(startScale, Vector3.one * 0.9f, curveValue);

            // Fade out
            buttonCanvasGroups[index].alpha = 1f - curveValue;

            yield return null;
        }

        // Finalizar
        buttonRects[index].anchoredPosition = targetPos;
        buttonRects[index].localScale = Vector3.one * 0.9f;
        buttonCanvasGroups[index].alpha = 0f;
    }

    private void StartIdleAnimations()
    {
        StopIdleAnimations();

        for (int i = 0; i < buttonRects.Count; i++)
        {
            Coroutine idleCoroutine = StartCoroutine(ButtonIdleFloat(buttonRects[i], i));
            buttonIdleCoroutines.Add(idleCoroutine);
        }
    }

    private void StopIdleAnimations()
    {
        foreach (Coroutine coroutine in buttonIdleCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        buttonIdleCoroutines.Clear();
    }

    private IEnumerator ButtonIdleFloat(RectTransform rect, int index)
    {
        Vector2 originalPos = buttonOriginalPositions[index];
        float time = index * 0.5f; // Offset para que no se muevan todos igual

        while (true)
        {
            time += Time.unscaledDeltaTime;

            // Movimiento flotante muy sutil
            float yOffset = Mathf.Sin(time * 1.5f) * 3f;
            rect.anchoredPosition = originalPos + Vector2.up * yOffset;

            yield return null;
        }
    }

    // Método helper para toggle
    public void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OnDestroy()
    {
        // Asegurar que el timeScale se restaure si se destruye el objeto
        if (isOpen)
            Time.timeScale = originalTimeScale;
    }
}