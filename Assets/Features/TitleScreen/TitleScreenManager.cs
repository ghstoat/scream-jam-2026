using NaughtyAttributes;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] Animator mechAnimator;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] CanvasGroup textCanvasGroup;
    [SerializeField] CanvasGroup fadeOutCanvasGroup;
    [SerializeField] GameObject saveFoundScreen;

    [SerializeField] float typewriterSpeed = 0.04f;

    private AsyncOperation asyncLoad;
    [SerializeField] string[] openingDialogue;

    private Coroutine dialogueCoroutine;
    private int currentLine;
    private bool isTyping;
    private bool dialogueStarted;
    private bool waitingForClickRelease;

    private bool saveFound;

    private void Start()
    {
        saveFound = false;  
        asyncLoad = SceneManager.LoadSceneAsync("Mech", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        if (PlayerPrefs.GetInt(GameManager.Checkpoint, 0) != 0)
        {
            saveFound = true;
            saveFoundScreen.SetActive(true);

        }
        else
        {
            StartCoroutine(StartTitle());
        }

    }

    public void Continue() //called from saveFoundScreen
    {
        asyncLoad.allowSceneActivation = true;
    }

    public void Restart() //called from saveFoundScreen
    {
        saveFound = false;
        PlayerPrefs.SetInt(GameManager.Checkpoint, 0);

        StartCoroutine(StartTitle());
    }


    private void Update()
    {
        if (saveFound) return;


        if (waitingForClickRelease)
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                waitingForClickRelease = false;
            }

            return;
        }

        if (dialogueStarted && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleDialogueClick();
        }
    }

    private IEnumerator StartTitle()
    {
        saveFoundScreen.SetActive(false);

        dialogueText.text = "Click to start";

        fadeOutCanvasGroup.alpha = 1;
        textCanvasGroup.alpha = 0;
        yield return new WaitForSeconds(1f);

        float fadeDuration = 2f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeOutCanvasGroup.alpha = Mathf.Clamp01((fadeDuration - timer )/ fadeDuration);
            yield return null;
        }
        fadeOutCanvasGroup.alpha = 0;
        yield return new WaitForSeconds(.5f);


        fadeDuration = 1f;
        timer = 0f;

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

        dialogueCoroutine = StartCoroutine(TypeLine(openingDialogue[currentLine]));
    }

    private void HandleDialogueClick()
    {
        if (isTyping)
        {
            if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);

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


    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

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