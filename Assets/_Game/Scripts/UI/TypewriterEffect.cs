using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

public class TypewriterEffect : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Text textComponent; // Para UI Text normal
    [SerializeField] private TextMeshProUGUI tmpTextComponent; // Para TextMeshPro

    [Header("Phrases")]
    [TextArea(3, 10)]
    [SerializeField]
    private string[] phrases = new string[]
    {
        "Hola <b>mundo</b>!",
        "Este es un texto\ncon salto de línea",
        "<b>Negrilla</b> y <i>cursiva</i>",
        "¡Prueba esto!"
    };

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // Tiempo entre cada carácter al escribir
    [SerializeField] private float deletingSpeed = 0.03f; // Tiempo entre cada carácter al borrar
    [SerializeField] private float delayBetweenPhrases = 1.5f; // Pausa después de escribir completo
    [SerializeField] private float delayBeforeDeleting = 0.5f; // Pausa antes de empezar a borrar

    [Header("Options")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool loop = true;

    private int currentPhraseIndex = 0;
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    private void Start()
    {
        if (autoStart)
            StartTypewriter();
    }

    [ContextMenu("Start Typewriter")]
    public void StartTypewriter()
    {
        if (isTyping) return;

        // Validar que haya un componente de texto
        if (textComponent == null && tmpTextComponent == null)
        {
            Debug.LogError("No hay componente de texto asignado!");
            return;
        }

        // Validar que haya frases
        if (phrases.Length == 0)
        {
            Debug.LogError("No hay frases en el array!");
            return;
        }

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterLoop());
    }

    [ContextMenu("Stop Typewriter")]
    public void StopTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        isTyping = false;
        SetText("");
    }

    private IEnumerator TypewriterLoop()
    {
        isTyping = true;
        currentPhraseIndex = 0;

        while (true)
        {
            string currentPhrase = phrases[currentPhraseIndex];

            // Escribir la frase
            yield return StartCoroutine(TypePhrase(currentPhrase));

            // Esperar antes de borrar
            yield return new WaitForSeconds(delayBeforeDeleting);

            // Borrar la frase
            yield return StartCoroutine(DeletePhrase(currentPhrase));

            // Esperar entre frases
            yield return new WaitForSeconds(delayBetweenPhrases);

            // Avanzar a la siguiente frase
            currentPhraseIndex++;

            // Si llegamos al final y no hay loop, terminar
            if (currentPhraseIndex >= phrases.Length)
            {
                if (loop)
                    currentPhraseIndex = 0;
                else
                    break;
            }
        }

        isTyping = false;
    }

    private IEnumerator TypePhrase(string phrase)
    {
        // Obtener los caracteres visibles (sin tags de Rich Text)
        string cleanPhrase = StripRichTextTags(phrase);

        // Construir el texto con tags mientras escribimos
        string displayedText = "";
        int visibleCharCount = 0;

        while (visibleCharCount < cleanPhrase.Length)
        {
            // Construir el texto con tags hasta el carácter visible actual
            displayedText = GetTextWithTagsUpToIndex(phrase, visibleCharCount);
            SetText(displayedText);

            visibleCharCount++;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Mostrar el texto completo al final
        SetText(phrase);
    }

    private IEnumerator DeletePhrase(string phrase)
    {
        // Obtener los caracteres visibles
        string cleanPhrase = StripRichTextTags(phrase);
        int visibleCharCount = cleanPhrase.Length;

        while (visibleCharCount > 0)
        {
            visibleCharCount--;

            // Construir el texto con tags hasta el carácter visible actual
            string displayedText = GetTextWithTagsUpToIndex(phrase, visibleCharCount);
            SetText(displayedText);

            yield return new WaitForSeconds(deletingSpeed);
        }

        SetText("");
    }

    // Elimina todos los tags de Rich Text para obtener solo el texto visible
    private string StripRichTextTags(string richText)
    {
        // Patrón regex para encontrar tags de Rich Text como <b>, </b>, <i>, </i>, etc.
        string pattern = @"<[^>]+>";
        return Regex.Replace(richText, pattern, "");
    }

    // Obtiene el texto con tags de Rich Text hasta un índice específico de caracteres visibles
    private string GetTextWithTagsUpToIndex(string richText, int visibleIndex)
    {
        if (visibleIndex <= 0)
            return "";

        string result = "";
        int visibleCount = 0;
        int i = 0;

        // Stack para rastrear los tags abiertos
        System.Collections.Generic.Stack<string> openTags = new System.Collections.Generic.Stack<string>();

        while (i < richText.Length && visibleCount < visibleIndex)
        {
            // Si encontramos un tag
            if (richText[i] == '<')
            {
                int tagEnd = richText.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    string tag = richText.Substring(i, tagEnd - i + 1);
                    result += tag;

                    // Si es un tag de apertura (no empieza con </)
                    if (!tag.StartsWith("</"))
                    {
                        // Extraer el nombre del tag (ej: "b" de "<b>")
                        string tagName = Regex.Match(tag, @"<(\w+)").Groups[1].Value;
                        if (!string.IsNullOrEmpty(tagName))
                            openTags.Push(tagName);
                    }
                    else
                    {
                        // Es un tag de cierre
                        if (openTags.Count > 0)
                            openTags.Pop();
                    }

                    i = tagEnd + 1;
                    continue;
                }
            }

            // Es un carácter visible
            result += richText[i];
            visibleCount++;
            i++;
        }

        // Cerrar todos los tags que quedaron abiertos
        while (openTags.Count > 0)
        {
            string tagName = openTags.Pop();
            result += "</" + tagName + ">";
        }

        return result;
    }

    // Helper para establecer el texto en el componente correcto
    private void SetText(string text)
    {
        if (tmpTextComponent != null)
            tmpTextComponent.text = text;
        else if (textComponent != null)
            textComponent.text = text;
    }

    // Método público para cambiar las frases en runtime
    public void SetPhrases(string[] newPhrases)
    {
        if (newPhrases == null || newPhrases.Length == 0)
        {
            Debug.LogWarning("El array de frases está vacío!");
            return;
        }

        phrases = newPhrases;

        // Reiniciar si está activo
        if (isTyping)
        {
            StopTypewriter();
            StartTypewriter();
        }
    }

    // Método público para cambiar la velocidad
    public void SetTypingSpeed(float speed)
    {
        typingSpeed = Mathf.Max(0.01f, speed);
    }

    public void SetDeletingSpeed(float speed)
    {
        deletingSpeed = Mathf.Max(0.01f, speed);
    }

    private void OnDestroy()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
    }
}