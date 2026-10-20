using NaughtyAttributes;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] Animator mechAnimator;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] CanvasGroup textCanvasGroup;
    [SerializeField] CanvasGroup fadeOutCanvasGroup;

    [SerializeField] float typewriterSpeed = 0.04f;

    private AsyncOperation asyncLoad;
    [SerializeField] string[] openingDialogue;

    private Coroutine dialogueCoroutine;
    private int currentLine;
    private bool isTyping;
    private bool dialogueStarted;
    private bool waitingForClickRelease;
    private void Start()
    {
        asyncLoad = SceneManager.LoadSceneAsync("Mech", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        dialogueText.text = "Click to start";

        StartCoroutine(StartTitle());
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        // Wait until the start click has been released.
        if (waitingForClickRelease)
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                waitingForClickRelease = false;
            }

            return;
        }

        if (dialogueStarted &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleDialogueClick();
        }
    }

    private IEnumerator StartTitle()
    {
        textCanvasGroup.alpha = 0;
        yield return new WaitForSeconds(1f);

        float fadeDuration = 1f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        while (Mouse.current == null ||
               !Mouse.current.leftButton.wasPressedThisFrame)
        {
            yield return null;
        }

        // Lock dialogue input until this click is released.
        waitingForClickRelease = true;

        StartDialogue();
    }

    [Button]
    private void StartDialogue()
    {
        if (dialogueStarted) return;
        if (dialogueCoroutine != null) return;

        dialogueStarted = true;
        currentLine = 0;

        dialogueCoroutine = StartCoroutine(DialogueRoutine());
    }

    private void HandleDialogueClick()
    {
        if (isTyping)
        {
            if (dialogueCoroutine != null)
                StopCoroutine(dialogueCoroutine);

            dialogueText.maxVisibleCharacters = int.MaxValue;
            isTyping = false;
            dialogueCoroutine = null;
        }
        else
        {
            currentLine++;

            if (currentLine >= openingDialogue.Length)
            {
                dialogueStarted = false;
                StartCoroutine(CrashRoutine());
                return;
            }

            dialogueCoroutine = StartCoroutine(
                TypeLine(openingDialogue[currentLine]));
        }
    }

    private IEnumerator DialogueRoutine()
    {
        if (openingDialogue.Length == 0)
        {
            dialogueStarted = false;
            yield return StartCoroutine(CrashRoutine());
            yield break;
        }

        yield return StartCoroutine(TypeLine(openingDialogue[currentLine]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

        // Ensure TMP has updated its character information.
        dialogueText.ForceMeshUpdate();

        int characterCount = dialogueText.textInfo.characterCount;

        for (int i = 1; i <= characterCount; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        dialogueCoroutine = null;
    }

    private IEnumerator CrashRoutine()
    {
        mechAnimator.SetTrigger("CRASH");

        float fadeDuration = 1f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Clamp01(fadeDuration - timer / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(2);

        fadeDuration = .5f;
        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeOutCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;
    }
}