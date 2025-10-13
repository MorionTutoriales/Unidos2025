using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Elements")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Button playButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private RectTransform logoTransform; // Logo del juego (opcional)

    [Header("About Screen Elements")]
    [SerializeField] private CanvasGroup aboutPanel;
    [SerializeField] private RectTransform aboutContentTransform; // Contenedor del texto/info
    [SerializeField] private Button backButton;
    [SerializeField] private RectTransform[] aboutParallaxLayers; // Capas para parallax

    [Header("Character Wave Settings")]
    [SerializeField] private float waveSpeed = 1.5f;
    [SerializeField] private float waveAmplitude = 8f;
    [SerializeField] private float waveRotation = 2f;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.8f;
    [SerializeField] private float buttonAnimationDuration = 0.5f;
    [SerializeField] private float delayBetweenButtons = 0.12f;
    [SerializeField] private float parallaxSpeed = 0.6f;

    [Header("Movement Distances")]
    [SerializeField] private float characterSlideDistance = 300f;
    [SerializeField] private float buttonSlideDistance = 200f;
    [SerializeField] private float logoSlideDistance = 150f;

    [Header("Parallax Multipliers")]
    [SerializeField] private float[] parallaxMultipliers = { 0.3f, 0.6f, 1.0f }; // Diferentes velocidades por capa

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve elasticCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    private RectTransform characterRect;
    private Vector2 characterOriginalPos;
    private Vector2 logoOriginalPos;

    private List<RectTransform> mainMenuButtons = new List<RectTransform>();
    private List<CanvasGroup> mainMenuButtonGroups = new List<CanvasGroup>();
    private List<Vector2> buttonOriginalPositions = new List<Vector2>();

    private Vector2[] parallaxOriginalPositions;
    private Vector2 aboutContentOriginalPos;
    private CanvasGroup aboutContentGroup;

    private bool isInAboutScreen = false;
    private bool isAnimating = false;
    private Coroutine waveCoroutine;

    private void Awake()
    {
        SetupComponents();
        SetupButtons();
        InitializeAboutScreen();
    }

    private void Start()
    {
        // Iniciar animación de wave del personaje
        StartCharacterWave();

        // Asegurar que todo esté en posiciones correctas antes de animar
        ResetAllPositions();

        // Animar entrada inicial del menú
        StartCoroutine(InitialMenuAnimation());
    }

    private void SetupComponents()
    {
        characterRect = characterImage.GetComponent<RectTransform>();
        characterOriginalPos = characterRect.anchoredPosition;

        if (logoTransform != null)
            logoOriginalPos = logoTransform.anchoredPosition;

        // Configurar botones del menú principal
        mainMenuButtons.Add(playButton.GetComponent<RectTransform>());
        mainMenuButtons.Add(aboutButton.GetComponent<RectTransform>());
        mainMenuButtons.Add(exitButton.GetComponent<RectTransform>());

        foreach (RectTransform rect in mainMenuButtons)
        {
            buttonOriginalPositions.Add(rect.anchoredPosition);

            CanvasGroup group = rect.GetComponent<CanvasGroup>();
            if (group == null)
                group = rect.gameObject.AddComponent<CanvasGroup>();
            mainMenuButtonGroups.Add(group);
        }

        // Guardar posiciones originales del parallax y configurar CanvasGroups
        if (aboutParallaxLayers.Length > 0)
        {
            parallaxOriginalPositions = new Vector2[aboutParallaxLayers.Length];
            for (int i = 0; i < aboutParallaxLayers.Length; i++)
            {
                parallaxOriginalPositions[i] = aboutParallaxLayers[i].anchoredPosition;

                // Asegurar que cada capa tenga un CanvasGroup y esté oculta inicialmente
                CanvasGroup layerGroup = aboutParallaxLayers[i].GetComponent<CanvasGroup>();
                if (layerGroup == null)
                    layerGroup = aboutParallaxLayers[i].gameObject.AddComponent<CanvasGroup>();
                layerGroup.alpha = 0;
            }
        }
    }

    private void SetupButtons()
    {
        playButton.onClick.AddListener(PlayGame);
        aboutButton.onClick.AddListener(ShowAbout);
        exitButton.onClick.AddListener(ExitGame);
        backButton.onClick.AddListener(BackToMainMenu);

        // Añadir efectos hover
        AddHoverEffect(playButton);
        AddHoverEffect(aboutButton);
        AddHoverEffect(exitButton);
        AddHoverEffect(backButton);
    }

    private void AddHoverEffect(Button button)
    {
        RectTransform rect = button.GetComponent<RectTransform>();

        UnityEngine.EventSystems.EventTrigger trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { StartCoroutine(ScaleButton(rect, 1.08f, 0.2f)); });
        trigger.triggers.Add(entryEnter);

        UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { StartCoroutine(ScaleButton(rect, 1.0f, 0.2f)); });
        trigger.triggers.Add(entryExit);
    }

    private IEnumerator ScaleButton(RectTransform rect, float targetScale, float duration)
    {
        Vector3 startScale = rect.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.localScale = Vector3.Lerp(startScale, endScale, elasticCurve.Evaluate(t));
            yield return null;
        }

        rect.localScale = endScale;
    }

    private void InitializeAboutScreen()
    {
        aboutPanel.alpha = 0;
        aboutPanel.blocksRaycasts = false;
        backButton.interactable = false;

        if (aboutContentTransform != null)
        {
            aboutContentOriginalPos = aboutContentTransform.anchoredPosition;
            aboutContentGroup = aboutContentTransform.GetComponent<CanvasGroup>();
            if (aboutContentGroup == null)
                aboutContentGroup = aboutContentTransform.gameObject.AddComponent<CanvasGroup>();
            aboutContentGroup.alpha = 0;
        }
    }

    private void ResetAllPositions()
    {
        // Resetear personaje
        characterRect.anchoredPosition = characterOriginalPos;
        CanvasGroup charGroup = characterImage.GetComponent<CanvasGroup>();
        if (charGroup == null)
            charGroup = characterImage.gameObject.AddComponent<CanvasGroup>();
        charGroup.alpha = 1f;

        // Resetear logo
        if (logoTransform != null)
        {
            logoTransform.anchoredPosition = logoOriginalPos;
            CanvasGroup logoGroup = logoTransform.GetComponent<CanvasGroup>();
            if (logoGroup == null)
                logoGroup = logoTransform.gameObject.AddComponent<CanvasGroup>();
            logoGroup.alpha = 1f;
        }

        // Resetear botones del menú principal
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            mainMenuButtons[i].anchoredPosition = buttonOriginalPositions[i];
            mainMenuButtonGroups[i].alpha = 1f;
        }

        // Resetear about panel
        aboutPanel.alpha = 0;
        aboutPanel.blocksRaycasts = false;

        // Resetear contenido del about
        if (aboutContentTransform != null)
        {
            aboutContentTransform.anchoredPosition = aboutContentOriginalPos;
            aboutContentGroup.alpha = 0;
        }

        // Resetear capas parallax
        if (aboutParallaxLayers.Length > 0 && parallaxOriginalPositions != null)
        {
            for (int i = 0; i < aboutParallaxLayers.Length; i++)
            {
                aboutParallaxLayers[i].anchoredPosition = parallaxOriginalPositions[i];
                CanvasGroup layerGroup = aboutParallaxLayers[i].GetComponent<CanvasGroup>();
                if (layerGroup == null)
                    layerGroup = aboutParallaxLayers[i].gameObject.AddComponent<CanvasGroup>();
                layerGroup.alpha = 0;
            }
        }
    }

    private void StartCharacterWave()
    {
        if (waveCoroutine != null)
            StopCoroutine(waveCoroutine);
        waveCoroutine = StartCoroutine(CharacterWaveAnimation());
    }

    private IEnumerator CharacterWaveAnimation()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime * waveSpeed;

            // Movimiento ondulante en Y
            float yOffset = Mathf.Sin(time) * waveAmplitude;

            // Rotación sutil
            float rotation = Mathf.Sin(time * 0.8f) * waveRotation;

            // Escala sutil (respiración)
            float scaleOffset = 1f + Mathf.Sin(time * 0.5f) * 0.02f;

            characterRect.anchoredPosition = characterOriginalPos + Vector2.up * yOffset;
            characterRect.localRotation = Quaternion.Euler(0, 0, rotation);
            characterRect.localScale = Vector3.one * scaleOffset;

            yield return null;
        }
    }

    private IEnumerator InitialMenuAnimation()
    {
        // Ocultar elementos inicialmente
        if (logoTransform != null)
        {
            logoTransform.anchoredPosition = logoOriginalPos + Vector2.up * 100f;
            CanvasGroup logoGroup = logoTransform.GetComponent<CanvasGroup>();
            if (logoGroup == null)
                logoGroup = logoTransform.gameObject.AddComponent<CanvasGroup>();
            logoGroup.alpha = 0;
        }

        foreach (CanvasGroup group in mainMenuButtonGroups)
        {
            group.alpha = 0;
        }

        // Animar logo
        if (logoTransform != null)
        {
            yield return StartCoroutine(AnimateLogoIn());
        }

        // Animar botones en cascada
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            StartCoroutine(AnimateMainMenuButtonIn(i));
            yield return new WaitForSeconds(delayBetweenButtons);
        }
    }

    private IEnumerator AnimateLogoIn()
    {
        CanvasGroup logoGroup = logoTransform.GetComponent<CanvasGroup>();
        float elapsed = 0f;
        Vector2 startPos = logoTransform.anchoredPosition;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = elasticCurve.Evaluate(t);

            logoTransform.anchoredPosition = Vector2.Lerp(startPos, logoOriginalPos, curveValue);
            logoGroup.alpha = curveValue;

            yield return null;
        }

        logoTransform.anchoredPosition = logoOriginalPos;
        logoGroup.alpha = 1f;
    }

    private IEnumerator AnimateMainMenuButtonIn(int index)
    {
        float elapsed = 0f;
        Vector2 startPos = buttonOriginalPositions[index] + Vector2.left * buttonSlideDistance;
        mainMenuButtons[index].anchoredPosition = startPos;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = elasticCurve.Evaluate(t);

            mainMenuButtons[index].anchoredPosition = Vector2.Lerp(startPos, buttonOriginalPositions[index], curveValue);
            mainMenuButtonGroups[index].alpha = curveValue;

            yield return null;
        }

        mainMenuButtons[index].anchoredPosition = buttonOriginalPositions[index];
        mainMenuButtonGroups[index].alpha = 1f;
    }

    [ContextMenu("Test Show About")]
    public void ShowAbout()
    {
        if (isInAboutScreen || isAnimating) return;
        StartCoroutine(TransitionToAbout());
    }

    [ContextMenu("Test Back To Menu")]
    public void BackToMainMenu()
    {
        if (!isInAboutScreen || isAnimating) return;
        StartCoroutine(TransitionToMainMenu());
    }

    private IEnumerator TransitionToAbout()
    {
        isAnimating = true;
        isInAboutScreen = true;

        // Deshabilitar botones del menú principal
        playButton.interactable = false;
        aboutButton.interactable = false;
        exitButton.interactable = false;

        // Activar panel de About
        aboutPanel.blocksRaycasts = true;

        // Animaciones simultáneas con diferentes timings
        StartCoroutine(SlideOutCharacter());
        StartCoroutine(SlideOutMainMenuButtons());
        if (logoTransform != null)
            StartCoroutine(SlideOutLogo());

        yield return new WaitForSeconds(transitionDuration * 0.5f);

        // Fade in del panel About
        StartCoroutine(FadeInAboutPanel());

        // Efecto parallax en las capas
        if (aboutParallaxLayers.Length > 0)
            StartCoroutine(ParallaxInAboutLayers());

        // Animar contenido del About
        yield return StartCoroutine(SlideInAboutContent());

        backButton.interactable = true;
        isAnimating = false;
    }

    private IEnumerator TransitionToMainMenu()
    {
        isAnimating = true;

        backButton.interactable = false;

        // Salir contenido de About con parallax
        StartCoroutine(SlideOutAboutContent());
        if (aboutParallaxLayers.Length > 0)
            StartCoroutine(ParallaxOutAboutLayers());

        yield return new WaitForSeconds(transitionDuration * 0.5f);

        // Fade out del panel About
        StartCoroutine(FadeOutAboutPanel());

        // Regresar elementos del menú principal
        StartCoroutine(SlideInCharacter());
        if (logoTransform != null)
            StartCoroutine(SlideInLogo());

        yield return new WaitForSeconds(transitionDuration * 0.3f);

        // Regresar botones
        yield return StartCoroutine(SlideInMainMenuButtons());

        playButton.interactable = true;
        aboutButton.interactable = true;
        exitButton.interactable = true;

        aboutPanel.blocksRaycasts = false;
        isInAboutScreen = false;
        isAnimating = false;
    }

    // Animaciones de salida del menú principal
    private IEnumerator SlideOutCharacter()
    {
        float elapsed = 0f;
        Vector2 startPos = characterRect.anchoredPosition;
        Vector2 targetPos = characterOriginalPos + Vector2.left * characterSlideDistance;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);

            characterRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);

            CanvasGroup charGroup = characterImage.GetComponent<CanvasGroup>();
            if (charGroup == null)
                charGroup = characterImage.gameObject.AddComponent<CanvasGroup>();
            charGroup.alpha = 1f - curveValue * 0.7f;

            yield return null;
        }
    }

    private IEnumerator SlideOutMainMenuButtons()
    {
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            StartCoroutine(SlideOutButton(i));
            yield return new WaitForSeconds(delayBetweenButtons * 0.5f);
        }
    }

    private IEnumerator SlideOutButton(int index)
    {
        float elapsed = 0f;
        Vector2 startPos = mainMenuButtons[index].anchoredPosition;
        Vector2 targetPos = buttonOriginalPositions[index] + Vector2.right * buttonSlideDistance;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = transitionCurve.Evaluate(t);

            mainMenuButtons[index].anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            mainMenuButtonGroups[index].alpha = 1f - curveValue;

            yield return null;
        }

        mainMenuButtonGroups[index].alpha = 0f;
    }

    private IEnumerator SlideOutLogo()
    {
        CanvasGroup logoGroup = logoTransform.GetComponent<CanvasGroup>();
        float elapsed = 0f;
        Vector2 startPos = logoTransform.anchoredPosition;
        Vector2 targetPos = logoOriginalPos + Vector2.up * logoSlideDistance;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);

            logoTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            logoGroup.alpha = 1f - curveValue;

            yield return null;
        }

        logoGroup.alpha = 0f;
    }

    // Animaciones de entrada del About
    private IEnumerator FadeInAboutPanel()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            aboutPanel.alpha = transitionCurve.Evaluate(t) * 0.95f;
            yield return null;
        }

        aboutPanel.alpha = 0.95f;
    }

    private IEnumerator ParallaxInAboutLayers()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);

            for (int i = 0; i < aboutParallaxLayers.Length; i++)
            {
                float multiplier = i < parallaxMultipliers.Length ? parallaxMultipliers[i] : 1f;
                Vector2 offset = Vector2.right * 500f * (1f - curveValue) * multiplier;
                aboutParallaxLayers[i].anchoredPosition = parallaxOriginalPositions[i] + offset;

                CanvasGroup layerGroup = aboutParallaxLayers[i].GetComponent<CanvasGroup>();
                if (layerGroup == null)
                    layerGroup = aboutParallaxLayers[i].gameObject.AddComponent<CanvasGroup>();
                layerGroup.alpha = curveValue;
            }

            yield return null;
        }
    }

    private IEnumerator SlideInAboutContent()
    {
        if (aboutContentTransform == null) yield break;

        float elapsed = 0f;
        Vector2 startPos = aboutContentOriginalPos + Vector2.right * 400f;
        aboutContentTransform.anchoredPosition = startPos;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = elasticCurve.Evaluate(t);

            aboutContentTransform.anchoredPosition = Vector2.Lerp(startPos, aboutContentOriginalPos, curveValue);
            aboutContentGroup.alpha = curveValue;

            yield return null;
        }

        aboutContentTransform.anchoredPosition = aboutContentOriginalPos;
        aboutContentGroup.alpha = 1f;
    }

    // Animaciones de salida del About
    private IEnumerator FadeOutAboutPanel()
    {
        float elapsed = 0f;
        float startAlpha = aboutPanel.alpha;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            aboutPanel.alpha = Mathf.Lerp(startAlpha, 0f, transitionCurve.Evaluate(t));
            yield return null;
        }

        aboutPanel.alpha = 0f;
    }

    private IEnumerator ParallaxOutAboutLayers()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);

            for (int i = 0; i < aboutParallaxLayers.Length; i++)
            {
                float multiplier = i < parallaxMultipliers.Length ? parallaxMultipliers[i] : 1f;
                Vector2 offset = Vector2.left * 500f * curveValue * multiplier;
                aboutParallaxLayers[i].anchoredPosition = parallaxOriginalPositions[i] + offset;

                CanvasGroup layerGroup = aboutParallaxLayers[i].GetComponent<CanvasGroup>();
                if (layerGroup != null)
                    layerGroup.alpha = 1f - curveValue;
            }

            yield return null;
        }
    }

    private IEnumerator SlideOutAboutContent()
    {
        if (aboutContentTransform == null) yield break;

        float elapsed = 0f;
        Vector2 startPos = aboutContentTransform.anchoredPosition;
        Vector2 targetPos = aboutContentOriginalPos + Vector2.left * 400f;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = transitionCurve.Evaluate(t);

            aboutContentTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            aboutContentGroup.alpha = 1f - curveValue;

            yield return null;
        }

        aboutContentTransform.anchoredPosition = targetPos;
        aboutContentGroup.alpha = 0f;
    }

    // Animaciones de regreso al menú principal
    private IEnumerator SlideInCharacter()
    {
        float elapsed = 0f;
        Vector2 startPos = characterRect.anchoredPosition;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = elasticCurve.Evaluate(t);

            characterRect.anchoredPosition = Vector2.Lerp(startPos, characterOriginalPos, curveValue);

            CanvasGroup charGroup = characterImage.GetComponent<CanvasGroup>();
            if (charGroup != null)
                charGroup.alpha = 0.3f + curveValue * 0.7f;

            yield return null;
        }

        CanvasGroup finalGroup = characterImage.GetComponent<CanvasGroup>();
        if (finalGroup != null)
            finalGroup.alpha = 1f;
    }

    private IEnumerator SlideInLogo()
    {
        CanvasGroup logoGroup = logoTransform.GetComponent<CanvasGroup>();
        float elapsed = 0f;
        Vector2 startPos = logoTransform.anchoredPosition;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = elasticCurve.Evaluate(t);

            logoTransform.anchoredPosition = Vector2.Lerp(startPos, logoOriginalPos, curveValue);
            logoGroup.alpha = curveValue;

            yield return null;
        }

        logoGroup.alpha = 1f;
    }

    private IEnumerator SlideInMainMenuButtons()
    {
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            StartCoroutine(SlideInButton(i));
            yield return new WaitForSeconds(delayBetweenButtons);
        }
    }

    private IEnumerator SlideInButton(int index)
    {
        float elapsed = 0f;
        Vector2 startPos = mainMenuButtons[index].anchoredPosition;

        while (elapsed < buttonAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAnimationDuration;
            float curveValue = elasticCurve.Evaluate(t);

            mainMenuButtons[index].anchoredPosition = Vector2.Lerp(startPos, buttonOriginalPositions[index], curveValue);
            mainMenuButtonGroups[index].alpha = curveValue;

            yield return null;
        }

        mainMenuButtons[index].anchoredPosition = buttonOriginalPositions[index];
        mainMenuButtonGroups[index].alpha = 1f;
    }

    // Funciones de los botones
    private void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (waveCoroutine != null)
            StopCoroutine(waveCoroutine);
    }
}